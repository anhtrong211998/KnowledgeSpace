﻿var knowledgeBaseController = function () {
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

            $.post(url, form.serialize()).done(function (response) {
                var content = $("#txt_new_comment_content").val();

                var template = $('#tmpl_comments').html();
                var newComment = Mustache.render(template, {
                    id: response.id,
                    content: content,
                    createDate: formatRelativeTime(),
                    ownerName: $('#hid_current_login_name').val()
                });
                $("#txt_new_comment_content").val('');
                $('#comment_list').prepend(newComment);
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
    }

    function loadComments(id) {
        $.get('/knowledgeBase/GetCommentByKnowledgeBaseId?knowledgeBaseId=' + id).done(function (response, statusText, xhr) {
            if (xhr.status === 200) {
                var currentUser = $('#hid_current_user_id').val();
                console.log(currentUser);
                var template = $('#tmpl_comments').html();
                var childrenTemplate = $('#tmpl_children_comments').html();
                if (response) {
                    var html = '';
                    $.each(response, function (index, item) {
                        var childrenHtml = '';
                        if (item.children.length > 0) {
                            $.each(item.children, function (childIndex, childItem) {
                                
                                childrenHtml += Mustache.render(childrenTemplate, {
                                    id: childItem.id,
                                    content: childItem.content,
                                    createDate: formatRelativeTime(childItem.createDate),
                                    ownerName: childItem.ownerName,
                                    ownerUserId: childItem.ownerUserId
                                });
                                if (currentUser != undefined && currentUser === childItem.ownerUserId) {
                                    var editmode = '';
                                    editmode += ' - <a class="comment-edit-link" href="#" id="editComment_' + childItem.id + '" data-commentid="' + childItem.id + '">Sửa</a>';
                                    editmode += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + childItem.id + '" data-commentid="' + childItem.id + '">Xóa</a>';
                                    childrenHtml = childrenHtml.replace("replace_" + childItem.id, editmode);
                                }
                                else {
                                    childrenHtml = childrenHtml.replace("replace_" + childItem.id,"");
                                }
                            });
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
                            var editmode = '';
                            editmode += ' - <a class="comment-edit-link" href="#" id="editComment_' + item.id + '" data-commentid="' + item.id +'">Sửa</a>';
                            editmode += ' - <a class="comment-delete-link" href="#" id="deleteComment_' + item.id + '" data-commentid="' + item.id +'">Xóa</a>';
                            html = html.replace("replace_" + item.id, editmode)
                        }
                        else {
                            html = html.replace("replace_" + item.id, "");
                        }
                    });
                    $('#comment_list').html(html);
                }
            }
        });
    }
};