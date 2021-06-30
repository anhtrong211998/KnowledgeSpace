var knowledgeBaseController = function () {
    this.initialize = function () {
        var kbId = parseInt($('#hid_knowledge_base_id').val());
        loadComments(kbId);
        registerEvents();
    };

    function registerEvents() {
        // this is the id of the form
        $("#commentform").submit(function (e) {
            e.preventDefault(); // avoid to execute the actual submit of the form.
            var form = $(this);
            var url = form.attr('action');
            var knowledgeBaseId = parseInt($('#hid_knowledge_base_id').val());

            $.post(url, form.serialize()).done(function (response) {
                var content = $("#txt_new_comment_content").val();

                var template = $('#tmpl_comments').html();
                var newComment = Mustache.render(template, {
                    id: response.id,
                    content: content,
                    createDate: formatRelativeTime(),
                    ownerName: $('#hid_current_login_name').val()
                });
                loadComments(knowledgeBaseId);
                var numberOfComments = parseInt($('#hid_number_comments').val()) + 1;
                $('#hid_number_comments').val(numberOfComments);
                $('#comments-title').text('(' + numberOfComments + ') bình luận');
            });
        });

        //Binding reply comment event
        $('body').on('click', '.comment-reply-link', function (e) {
            e.preventDefault();
            var commentId = $(this).data('commentid');

            var knowledgeBaseId = parseInt($('#hid_knowledge_base_id').val());

            var template = $('#tmpl_reply_comment').html();
            var html = Mustache.render(template, {
                commentId: commentId
            });
            $('#reply_comment_' + commentId).html(html);

            $('body').on('click', "#btn_cancel_" + commentId, function (e) {
                e.preventDefault();
                $('#reply_comment_' + commentId).html('');
            });

            // this is the id of the form
            $("#frm_reply_comment_" + commentId).submit(function (e) {
                e.preventDefault(); // avoid to execute the actual submit of the form.
                var form = $(this);
                var url = form.attr('action');

                $.post(url, form.serialize()).done(function (response) {
                    //Reset reply comment
                    $("#txt_reply_content_" + commentId).val('');
                    $('#reply_comment_' + commentId).html('');

                    loadComments(knowledgeBaseId);
                    //Update number of comment
                    var numberOfComments = parseInt($('#hid_number_comments').val()) + 1;
                    $('#hid_number_comments').val(numberOfComments);
                    $('#comments-title').text('(' + numberOfComments + ') bình luận');
                });
            });
        });

        //Binding edit comment event
        $('body').on('click', '.comment-edit-link', function (e) {
            e.preventDefault();
            var commentId = $(this).data('commentid');
            var knowledgeBaseId = parseInt($('#hid_knowledge_base_id').val());
            var html = '';
            $.get('/knowledgeBase/GetCommentDetail?knowledgeBaseId=' + knowledgeBaseId + '&commentId=' + commentId)
                .done(function (response, statusText, xhr) {
                if (xhr.status === 200) {
                    var template = $('#tmpl_edit_comment').html();
                    html = Mustache.render(template, {
                        commentId: response.id,
                        content: response.content
                    });
                    $('#reply_comment_' + commentId).html(html);
                }
                    
            });                     

            $('body').on('click', "#btn_cancel_" + commentId, function (e) {
                e.preventDefault();
                $('#reply_comment_' + commentId).html('');
            });

            // this is the id of the form
            $('body').on('submit',"#frm_edit_comment_" + commentId,function (e) {
                e.preventDefault(); // avoid to execute the actual submit of the form.
                var form = $(this);
                
                var url = "/knowledgebase/EditComment?commentId=" + commentId;
                console.log(url);

                $.post(url, form.serialize()).done(function (response) {
                    if (response === true) {
                        loadComments(knowledgeBaseId);
                    }
                    

                });
            });
        });

        $('body').on('click', '.comment-delete-link', function (e) {
            e.preventDefault();
            var commentId = $(this).data('commentid');
            var knowledgeBaseId = parseInt($('#hid_knowledge_base_id').val());
            if (confirm("Bạn có muốn xóa bình luận này không?")) {
                $.get('/knowledgeBase/DeleteComment?knowledgeBaseId=' + knowledgeBaseId + '&commentId=' + commentId)
                    .done(function (response, statusText, xhr) {
                        if (xhr.status === 200) {
                            loadComments(knowledgeBaseId);
                        }

                    }); 
            }
        });


        $('#frm_vote').submit(function (e) {
            e.preventDefault();
            var form = $(this);
            $.post('/knowledgeBase/postVote', form.serialize()).done(function (response) {
                $('.like-it').text(response);
                $('.like-count').text(response);
            });
        });
        $('#frm_vote .like-it').click(function () {
            $('#frm_vote').submit();
        });

        $('#btn_send_report').off('click').on('click', function (e) {
            e.preventDefault();
            var form = $('#frm_report');
            $.post('/knowledgeBase/postReport', form.serialize())
                .done(function () {
                    $('#reportModal').modal('hide');
                    $('#txt_report_content').val('');
                });
        });

        $('body').on('click', '#comment-pagination', function (e) {
            e.preventDefault();
            var kbId = parseInt($('#hid_knowledge_base_id').val());
            var nextPageIndex = parseInt($(this).data('page-index')) + 1;
            $(this).data('page-index', nextPageIndex);
            loadComments(kbId, nextPageIndex);
        });

        $('body').on('click', '.replied-comment-pagination', function (e) {
            e.preventDefault();
            var kbId = parseInt($('#hid_knowledge_base_id').val());

            var commentId = parseInt($(this).data('id'));
            var nextPageIndex = parseInt($(this).data('page-index')) + 1;
            $(this).data('page-index', nextPageIndex);
            loadRepliedComments(kbId, commentId, nextPageIndex);
        });
    }

    function loadComments(id, pageIndex) {
        if (pageIndex === undefined) pageIndex = 1;
        $.get('/knowledgeBase/GetCommentsByKnowledgeBaseId?knowledgeBaseId=' + id + '&pageIndex=' + pageIndex)
            .done(function (response, statusText, xhr) {
            if (xhr.status === 200) {
                var currentUser = $('#hid_current_user_id').val();
                var KbOwner = $('#hid_knowledge_base_owner').val();
                var template = $('#tmpl_comments').html();
                var childrenTemplate = $('#tmpl_children_comments').html();
                if (response && response.items) {
                    var html = '';
                    $.each(response.items, function (index, item) {
                        var childrenHtml = '';
                        var editmode = '';
                        var editmodechild = '';
                        if (item.children && item.children.items) {
                            $.each(item.children.items, function (childIndex, childItem) {
                                
                                childrenHtml += Mustache.render(childrenTemplate, {
                                    id: childItem.id,
                                    content: childItem.content,
                                    createDate: formatRelativeTime(childItem.createDate),
                                    ownerName: childItem.ownerName,
                                    ownerUserId: childItem.ownerUserId
                                });
                                if (currentUser != undefined && currentUser === childItem.ownerUserId) {
                                    
                                    editmodechild += ' - <a class="comment-edit-link" href="#" id="editComment_' + childItem.id + '" data-commentid="' + childItem.id + '">Sửa</a>';
                                    editmodechild += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + childItem.id + '" data-commentid="' + childItem.id + '">Xóa</a>';
                                    
                                }
                                else if (currentUser != undefined && KbOwner != undefined && currentUser === KbOwner || currentUser != undefined && currentUser === item.ownerUserId) {
                                    editmodechild += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + childItem.id + '" data-commentid="' + childItem.id + '">Xóa</a>';
                                }
                                childrenHtml = childrenHtml.replace("replace_" + childItem.id, editmodechild);
                            });
                        }
                        if (response.pageIndex < response.pageCount) {
                            childrenHtml += '<a href="#" class="replied-comment-pagination" id="replied-comment-pagination-' + item.id + '" data-page-index="1" data-id="' + item.id + '">Xem thêm bình luận</a>';
                        }
                        else {
                            childrenHtml += '<a href="#" class="replied-comment-pagination" id="replied-comment-pagination-' + item.id + '" data-page-index="1" data-id="' + item.id + '" style="display:none">Xem thêm bình luận</a>';
                        }
                        html += Mustache.render(template, {
                            childrenHtml: childrenHtml,
                            id: item.id,
                            content: item.content,
                            createDate: formatRelativeTime(item.createDate),
                            ownerName: item.ownerName,
                            ownerUserId: item.ownerUserId
                        });
                        
                        if (currentUser != undefined && currentUser === item.ownerUserId) {
                           
                            editmode += ' - <a class="comment-edit-link" href="#" id="editComment_' + item.id + '" data-commentid="' + item.id + '">Sửa</a>'; 
                            editmode += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + item.id + '" data-commentid="' + item.id + '">Xóa</a>';
                        }
                        else if (currentUser != undefined && KbOwner != undefined && currentUser === KbOwner) {
                            editmode += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + item.id + '" data-commentid="' + item.id + '">Xóa</a>';
                        }
                        html = html.replace("replace_" + item.id, editmode)

                    });
                    $('#comment_list').append(html);
                    if (response.pageIndex < response.pageCount) {
                        $('#comment-pagination').show();
                    }
                    else {
                        $('#comment-pagination').hide();
                    }
                }
            }
        });
    }

    function loadRepliedComments(id, rootCommentId, pageIndex) {
        if (pageIndex === undefined) pageIndex = 1;
        $.get('/knowledgeBase/GetRepliedCommentsByKnowledgeBaseId?knowledgeBaseId=' + id + '&rootcommentId=' + rootCommentId
            + '&pageIndex=' + pageIndex)
            .done(function (response, statusText, xhr) {
                if (xhr.status === 200) {
                    var currentUser = $('#hid_current_user_id').val();
                    var KbOwner = $('#hid_knowledge_base_owner').val();
                    var template = $('#tmpl_children_comments').html();
                    var parent_comment_user = $('#parent_comment_user_' + id).val(); 
                    if (response && response.items) {
                        var html = '';
                        var editmode = '';
                        var editmodechild = '';
                        $.each(response.items, function (index, item) {
                            html += Mustache.render(template, {
                                id: item.id,
                                content: item.content,
                                ownerUserId: item.ownerUserId,
                                createDate: formatRelativeTime(item.createDate),
                                ownerName: item.ownerName
                            });
                            console.log(item.ownerUserId);
                            if (currentUser != undefined && currentUser === item.ownerUserId) {
                                editmode += ' - <a class="comment-edit-link" href="#" id="editComment_' + item.id + '" data-commentid="' + item.id + '">Sửa</a>';
                                editmode += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + item.id + '" data-commentid="' + item.id + '">Xóa</a>';
                            }
                            else if (currentUser != undefined && KbOwner != undefined && currentUser === KbOwner || currentUser != undefined && currentUser === parent_comment_user) {
                                editmode += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + item.id + '" data-commentid="' + item.id + '">Xóa</a>';
                            }
                            
                            html = html.replace("replace_" + item.id, editmode);
                        });

                        
                        $('#children_comments_' + rootCommentId).append(html);
                        if (response.pageIndex < response.pageCount) {
                            $('#replied-comment-pagination-' + rootCommentId).show();
                        }
                        else {
                            $('#replied-comment-pagination-' + rootCommentId).hide();
                        }
                    }
                }
            });
    }
};