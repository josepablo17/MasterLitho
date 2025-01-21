function mostrarNotificacionExito(event, element) {
    event.preventDefault();

    const dataId = element.getAttribute('data-id');


    Swal.fire({
        position: "center",
        icon: "success",
        title: "¡Se ha cambiado la contraseña correctamente!",
        showConfirmButton: false,
        timer: 1500
    }).then(() => {

        element.closest('form').submit();
    });
}

function toggleVisibility(id) {
    var input = document.getElementById(id);
    if (input.type === "password") {
        input.type = "text";
    } else {
        input.type = "password";
    }
}