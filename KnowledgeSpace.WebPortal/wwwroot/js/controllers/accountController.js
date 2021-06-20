var accountController = function () {
    this.initialize = function () {
        registerEvents();
    };

    function registerEvents() {
        CKEDITOR.replace('txt_problem');
        CKEDITOR.replace('txt_note');

        CKEDITOR.on('instanceReady', function () {
            $.each(CKEDITOR.instances, function (instance) {
                CKEDITOR.instances[instance].document.on("keyup", CK_jQ);
                CKEDITOR.instances[instance].document.on("paste", CK_jQ);
                CKEDITOR.instances[instance].document.on("keypress", CK_jQ);
                CKEDITOR.instances[instance].document.on("blur", CK_jQ);
                CKEDITOR.instances[instance].document.on("change", CK_jQ);
            });
        });

        $('#btn_add_attachment').off('click').on('click', function () {

            //// declare html variable includes html that need render on elemet with renderHTMLAnswer class
            var html = '';
            html += '<div class="input-group">';
            html += '<div class="input-group-area">';
            html += '<input type="text" class="form-control txtAttachment" placeholder="Type your attachment" name="txtAttachment">';
            html += '</div>'
            html += '<div class="input-group-append">';
            html += '<button class="btn btn-info btnAttachment" type="button">Browser</button>';
            html += '</div>'
            html += '<div class="input-group-append">';
            html += '<button class="btn btn-info btn_remove_attachment" type="button" style="width:33px;">-</button>';
            html += '</div>';
            html += '<p><input type="file" class="selectFile" name="attachments" style="display:none;" /></p>';
            html += ' </div>';

            //// render html inside element with class="renderHTMLAnswers"
            $('#render-attachment').prepend(html);
            return false;
        });

        $('body').on('click', '.btn_remove_attachment', function () {
            $(this).parent().parent().remove();
        });

        $('body').on('click', '.btnAttachment', function (){
            if ($(this).parent().parent().find($('input[name="attachments"]')).click()) {
                $(this).parent().parent().find($('input[name="attachments"]')).on('change', function (evt) {
                    //// set value of input with id="ChooseCV" is name of file
                    $(this).parent().parent().find($('input[name="txtAttachment"]')).val(evt.target.files[0].name);
                });
            }
            return false;
        });

        $("#frm_new_kb").submit(function (e) {
            e.preventDefault(); // avoid to execute the actual submit of the form.

            var form = $(this);
            form.validate();

            if (form.valid()) {
                var url = form.attr('action');
                var formData = false;
                if (window.FormData) {
                    formData = new FormData(form[0]);
                }

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (data) {
                        window.location.href = '/my-kbs';
                    },
                    enctype: 'multipart/form-data',
                    processData: false,  // Important!
                    contentType: false,
                    cache: false,
                });
            }
        });

        $("#frm_edit_kb").submit(function (e) {
            e.preventDefault(); // avoid to execute the actual submit of the form.

            var form = $(this);
            form.validate();

            if (form.valid()) {
                var url = form.attr('action');
                var formData = false;
                if (window.FormData) {
                    formData = new FormData(form[0]);
                }

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (data) {
                        window.location.href = '/my-kbs';
                    },
                    enctype: 'multipart/form-data',
                    processData: false,  // Important!
                    contentType: false,
                    cache: false,
                });
            }
        });
    }

    function CK_jQ() {
        for (instance in CKEDITOR.instances) {
            CKEDITOR.instances[instance].updateElement();
        }
    }
};