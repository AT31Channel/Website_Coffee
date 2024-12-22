function addToCart(form) {
    $.ajax({
        url: form.action,
        method: 'POST',
        data: $(form).serialize(),
        success: function (response) {
            // Hiển thị thông báo
            showNotification(response.message, response.success);

            if (response.success) {
                // Cập nhật số lượng trong giỏ hàng
                updateCartBadge(response.totalItems);
                
                // Refresh dropdown giỏ hàng
                refreshCartDropdown();
            }
        },
        error: function () {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại sau.', false);
        }
    });
    return false;
}

function showNotification(message, isSuccess) {
    var toast = $('<div class="toast" role="alert">')
        .append($('<div class="toast-header">')
            .append($('<i class="bi ' + (isSuccess ? 'bi-check-circle-fill text-success' : 'bi-x-circle-fill text-danger') + ' me-2">'))
            .append($('<strong class="me-auto">').text('Thông báo'))
            .append($('<button type="button" class="btn-close" data-bs-dismiss="toast">')))
        .append($('<div class="toast-body">').text(message));

    var container = $('<div class="notification-toast">').append(toast);
    $('body').append(container);

    var bsToast = new bootstrap.Toast(toast[0], { delay: 3000 });
    bsToast.show();

    toast.on('hidden.bs.toast', function () {
        container.remove();
    });
}

function updateCartBadge(count) {
    var badge = $('#cartDropdown .badge');
    if (count > 0) {
        if (badge.length === 0) {
            $('#cartDropdown').append($('<span class="badge bg-danger rounded-pill">').text(count));
        } else {
            badge.text(count);
        }
    } else {
        badge.remove();
    }
}

function refreshCartDropdown() {
    $.get('/Cart/GetCartDropdown', function(response) {
        $('#cartDropdownContainer').html(response);
    });
}

// Đăng ký sự kiện khi dropdown mở
$(document).on('show.bs.dropdown', '#cartDropdownContainer', function () {
    refreshCartDropdown();
});
