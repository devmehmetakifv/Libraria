$('.delete-user-btn').click(function () {
    var userId = $(this).data('user-id');
    $('#deleteUserId').val(userId);
    $('#deleteUserModal').modal('show');
});

$('#deleteUserForm').submit(function (e) {
    $('#deleteUserModal').modal('hide');
});

$('.delete-book-btn').click(function () {
    var courseId = $(this).data('book-id');
    $('#deleteBookId').val(courseId);
    $('#deleteBookModal').modal('show');
});

$('#deleteUserForm').submit(function (e) {
    $('#deleteUserModal').modal('hide');
});

//$('.delete-program-btn').click(function () {
//    var programId = $(this).data('program-id');
//    $('#deleteProgramId').val(programId);
//    $('#deleteProgramModal').modal('show');
//});

//$('#deleteProgramForm').submit(function (e) {
//    $('#deleteProgramModal').modal('hide');
//});

//$('.delete-department-btn').click(function () {
//    var departmentId = $(this).data('department-id');
//    $('#deleteDepartmentId').val(departmentId);
//    $('#deleteDepartmentModal').modal('show');
//});

//$('#deleteDepartmentForm').submit(function (e) {
//    $('#deleteDepartmentModal').modal('hide');
//})