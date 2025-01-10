var departments = [];
var programs = [];

$(document).ready(function () {
    // 1: Admin
    // 2: Student
    // 3: Instructor
    $('#bringUserForm').on('submit', function (e) {
        e.preventDefault();

        var formData = {
            email: $('#email').val(),
        };

        $.ajax({
            type: 'POST',
            url: '/Panel/BringUser',
            data: formData,
            success: function (response) {
                console.log(response)
                if (response.success) {
                    $('#bringUserPanel').hide();

                    $('#fillUserId').val(response.data.userID);
                    $('#edit-title').text("User " + response.data.name);

                    $('#fillUserEditName').val(response.data.name);
                    $('#fillUserEditSurname').val(response.data.surname);
                    $('#fillUserEditEmail').val(response.data.email);

                } else {
                    alert(response.message);
                }
                $('#userDetails').show();
            },
            error: function () {
                alert('Error fetching user.');
            }
        });
    });
});

// AJAX to fetch categories
$(document).ready(function () {
    $.ajax({
        type: 'POST',
        url: '/Panel/GetCategories',
        success: function (response) {
            if (response.success) {
                categories = response.data;
                console.log(categories)
            }
            else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Error fetching categories.');
        }
    });
});

// AJAX to fetch genres
$(document).ready(function () {
    $.ajax({
        type: 'POST',
        url: '/Panel/GetGenres',
        success: function (response) {
            if (response.success) {
                genres = response.data;
            }
            else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Error fetching genres.');
        }
    });
});

// AJAX to fetch authors
$(document).ready(function () {
    $.ajax({
        type: 'POST',
        url: '/Panel/GetAuthors',
        success: function (response) {
            if (response.success) {
                authors = response.data;
            }
            else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Error fetching authors.');
        }
    });
});

$(document).ready(function () {
    $('#bringBookForm').on('submit', function (e) {
        e.preventDefault();

        var formData = {
            bookTitle: $('#inputBookTitle').val(),
        };
        console.log(formData)
        $.ajax({
            type: 'POST',
            url: '/Panel/BringBook',
            data: formData,
            success: function (response) {
                if (response.success) {
                    console.log(response.data)
                    var categoryName = categories.find(d => d.categoryID === response.data.categoryID)?.categoryName;
                    console.log(categoryName)
                    console.log(response.data.bookID)
                    $('#bringBookPanel').hide();

                    $('#fillBookManagementId').val(response.data.bookID);
                    $('#edit-title').text(response.data.title);

                    $('#fillBookTitle').val(response.data.title);
                    $('#fillBookISBN').val(response.data.isbn);
                    $('#fillPublicationDate').val(response.data.publicationDate);
                    $('#fillStockQuantity').val(response.data.stockQuantity);
                    $('#fillBookCategoryId').val(categoryName);

                    $('#bookDetails').show();
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Error fetching book.');
            }
        });
    });
});