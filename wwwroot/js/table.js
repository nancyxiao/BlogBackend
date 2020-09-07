$(function () {
    // Activate tooltip
    $('[data-toggle="tooltip"]').tooltip();

    // Select/Deselect checkboxes
    //因使用ajax post回來時會刷洗view-all的html內容
    //故宣告callJS function 方便重新呼叫js
    callJS = () => {
        var checkbox = $('table tbody input[type="checkbox"]');
        $("#selectAll").on('click', function () {
            if (this.checked) {
                checkbox.each(function () {
                    this.checked = true;
                });
            } else {
                checkbox.each(function () {
                    this.checked = false;
                });
            }
        });

        checkbox.click(function () {
            if (!this.checked) {
                $("#view-all #selectAll").prop("checked", false);
            }
        });
    }

    callJS();

    showInPopup = (url, title) => {
        $.ajax({
            type: 'GET',
            url: url,
            success: function (res) {
                //debugger
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
            },
            error: function (err) {
                console.log(err);
            }
        });
    }



    jQueryAjaxPost = form => {
        try {
            $.ajax({
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function (res) {
                    //debugger
                    if (res.isValid) {
                        $('#view-all').html(res.html);
                        $('#form-modal .modal-body').html('');
                        $('#form-modal .modal-title').html('');
                        $('#form-modal').modal('hide');
                        callJS();
                    } else {
                        $('#form-modal .modal-body').html(res.html);
                    }
                },
                error: function (err) {
                    //debugger
                    console.log(err);
                }
            });

            return false;
        } catch (ex) {
            console.log(ex);
        }
    }

    jQueryAjaxDelete = (form, id) => {
        try {
            //debugger
            var ids = [];
            if (id == "") {
                $("input[name='options[]']:checked").each(function (index, obj) {
                    ids.push(obj.value);
                });
            }
            else {
                ids.push(id);
            }

            $.ajax({
                type: 'POST',
                url: form.action,
                data: JSON.stringify(ids),
                contentType: 'application/json; charset=utf-8',
                processData: true,
                success: function (res) {
                    //debugger
                    $('#view-all').html(res.html);
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                    callJS();
                },
                error: function (xhr, textStatus, err) {
                    console.log(err);
                }
            });
        } catch (ex) {
            console.log(ex);
        }
        return false;
    }
});