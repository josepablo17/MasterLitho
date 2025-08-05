function confirmarEnvio(mensaje, formId = null, callback = null) {
    const form = document.getElementById(formId);

    if (form && !form.checkValidity()) {
        form.reportValidity(); 
        return; 
    }

    Swal.fire({
        title: "¿Estás seguro?",
        text: mensaje || "Una vez hecha la acción, no podrá deshacerla.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#d33",
        confirmButtonText: "Sí, continuar",
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            if (formId) {
                form.submit();
            } else if (typeof callback === "function") {
                callback();
            }
        }
    });
}


// Función para mostrar una alerta de éxito
function alertaExito(mensaje) {
    Swal.fire({
        title: "¡Éxito!",
        text: mensaje || "Procesando tu solicitud.",
        icon: "success",
        confirmButtonColor: "#28a745",
    });
}

// Función para mostrar una alerta de error
function alertaError(mensaje) {
    Swal.fire({
        title: "¡Error!",
        text: mensaje || "Ocurrió un error inesperado.",
        icon: "error",
        confirmButtonColor: "#d33",
    });
}

// Función para mostrar una alerta de información
function alertaInfo(mensaje) {
    Swal.fire({
        title: "Información",
        text: mensaje || "Aquí hay algo que deberías saber.",
        icon: "info",
        confirmButtonColor: "#3085d6",
    });
}
