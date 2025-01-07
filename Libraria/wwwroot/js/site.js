$('.reply-ticket-btn').click(function () {
    console.log("Work!");
    var ticketId = $(this).data('ticket-id');
    var userNameWhoReplies = $(this).data('user-name');
    $('#replyTicketId').val(ticketId);
    $('#userNameWhoReplies').val(userNameWhoReplies);
    $('#replyTicketModal').modal('show');
});

$('#replyTicketForm').submit(function (e) {
    $('#replyTicketModal').modal('hide');
});

$('.delete-user-btn').click(function () {
    var userId = $(this).data('user-id');
    $('#deleteUserId').val(userId);
    $('#deleteUserModal').modal('show');
});

$('#deleteUserForm').submit(function (e) {
    $('#deleteUserModal').modal('hide');
});

$('.delete-course-btn').click(function () {
    var courseId = $(this).data('course-id');
    $('#deleteCourseId').val(courseId);
    $('#deleteCourseModal').modal('show');
});

$('#deleteUserForm').submit(function (e) {
    $('#deleteUserModal').modal('hide');
});

$('.delete-program-btn').click(function () {
    var programId = $(this).data('program-id');
    $('#deleteProgramId').val(programId);
    $('#deleteProgramModal').modal('show');
});

$('#deleteProgramForm').submit(function (e) {
    $('#deleteProgramModal').modal('hide');
});

$('.delete-department-btn').click(function () {
    var departmentId = $(this).data('department-id');
    $('#deleteDepartmentId').val(departmentId);
    $('#deleteDepartmentModal').modal('show');
});

$('#deleteDepartmentForm').submit(function (e) {
    $('#deleteDepartmentModal').modal('hide');
})