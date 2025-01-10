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

document.addEventListener('DOMContentLoaded', function () {
    $('#bookAuthorBirthDate').datepicker({
        format: "yyyy-mm-dd", // Adjust format as needed
        autoclose: true,
        todayHighlight: true,
    });
});
document.addEventListener('DOMContentLoaded', function () {
    $('#bookPublicationDate').datepicker({
        format: "yyyy-mm-dd", // Adjust format as needed
        autoclose: true,
        todayHighlight: true,
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const deleteButtons = document.querySelectorAll(".delete-book-btn");

    deleteButtons.forEach(button => {
        const bookId = button.getAttribute("data-book-id");
        const loanWarning = button.nextElementSibling; // Target the sibling warning

        // Function to check loan status (scoped to this button)
        function checkLoanStatus(bookId, buttonElement, warningElement) {
            fetch(`/api/PanelApi/HasLoans/${bookId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Server responded with status ${response.status}`);
                    }
                    return response.json();
                })
                .then(hasLoans => {
                    if (hasLoans) {
                        buttonElement.disabled = true;
                        warningElement.style.display = "inline";
                    } else {
                        buttonElement.disabled = false;
                        warningElement.style.display = "none";
                    }
                })
                .catch(error => console.error("Error checking loan status:", error));
        }

        // Check loan status on page load for this specific book
        checkLoanStatus(bookId, button, loanWarning);
    });
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