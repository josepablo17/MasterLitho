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

function toggleIconVisibility(inputId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(`icon-${inputId}`);

    if (input && icon) {
        if (input.value.trim() !== "") {
            icon.style.display = "flex"; // Muestra el ícono
        } else {
            icon.style.display = "none"; // Oculta el ícono
        }
    }
}

function toggleVisibility(inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        input.type = input.type === "password" ? "text" : "password";
    }
}
