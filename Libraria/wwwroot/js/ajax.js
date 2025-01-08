var departments = [];
var programs = [];

$(document).ready(function () {
    // 1: Admin
    // 2: Student
    // 3: Instructor
    $('#bringUserForm').on('submit', function (e) {
        e.preventDefault();

        var formData = {
            firstName: $('#inputFirstName').val(),
            lastName: $('#inputLastName').val()
        };

        $.ajax({
            type: 'POST',
            url: '/Panel/BringUser',
            data: formData,
            success: function (response) {
                if (response.success) {
                    $('#bringUserPanel').hide();

                    var departmentName = departments.find(d => d.id === response.data.departmentID)?.name;
                    var programName = programs.find(p => p.id === response.data.programID)?.name;
                    $('#fillUserId').val(response.data.id);

                    switch (response.data.roleId) {
                        case 1:
                            $('#edit-title').text("Admin " + response.data.firstName);

                            $('#fillHireDateDiv').remove();
                            $('#fillDepartmentNameDiv').remove();
                            $('#fillProgramNameDiv').remove();

                            $('#fillFirstName').val(response.data.firstName);
                            $('#fillLastName').val(response.data.lastName);
                            $('#fillGender').val(response.data.gender);
                            $('#fillContact').val(response.data.contact);
                            $('#fillDateOfBirth').val(response.data.dateOfBirth);
                            $('#fillPhoneNumber').val(response.data.phoneNumber);
                            $('#fillEmail').val(response.data.email);
                            break;
                        case 2:
                            $('#edit-title').text("Student " + response.data.firstName);

                            $('#fillHireDateDiv').remove();
                            $('#fillDepartmentNameDiv').remove();

                            $('#fillFirstName').val(response.data.firstName);
                            $('#fillLastName').val(response.data.lastName);
                            $('#fillGender').val(response.data.gender);
                            $('#fillContact').val(response.data.contact);
                            $('#fillDateOfBirth').val(response.data.dateOfBirth);
                            $('#fillPhoneNumber').val(response.data.phoneNumber);
                            $('#fillEmail').val(response.data.email);
                            $('#fillProgramName').val(programName);
                            break;
                        case 3:
                            $('#edit-title').text("Instructor " + response.data.firstName);

                            $('#fillProgramNameDiv').remove();

                            $('#fillFirstName').val(response.data.firstName);
                            $('#fillLastName').val(response.data.lastName);
                            $('#fillGender').val(response.data.gender);
                            $('#fillContact').val(response.data.contact);
                            $('#fillDateOfBirth').val(response.data.dateOfBirth);
                            $('#fillHireDate').val(response.data.hireDate);
                            $('#fillPhoneNumber').val(response.data.phoneNumber);
                            $('#fillEmail').val(response.data.email);
                            $('#fillDepartmentName').val(departmentName);
                            break;
                        default:
                    }
                    $('#userDetails').show();
                } else {
                    alert(response.message);
                }
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
            console.log(response)
            if (response.success) {
                categories = response.data;
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
            console.log(response)
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
            console.log(response)
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
                    $('#bringBookPanel').hide();

                    $('#fillBookId').val(response.data.bookID);
                    $('#edit-title').text(response.data.title);

                    $('#fillBookTitle').val(response.data.title);
                    $('#fillBookISBN').val(response.data.isbn);
                    $('#fillPublicationDate').val(response.data.publicationDate);
                    $('#fillStockQuantity').val(response.data.stockQuantity);

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