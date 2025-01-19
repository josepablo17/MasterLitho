document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        selectable: false,
        editable: false,
        dateClick: function (info) {
            const fechaSeleccionadaObj = new Date(info.dateStr);
            const fechaActual = new Date();
            fechaActual.setHours(0, 0, 0, 0);

            if (fechaSeleccionadaObj < fechaActual) {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "No se puede agendar en una fecha que ya pasó!"
                });
                $('#guardarEvento').prop('disabled', true); // Deshabilitar el botón de guardar
            } else {
                $('#guardarEvento').prop('disabled', false); // Habilitar el botón de guardar
                $('#crearEventoModal').modal('show');
                document.getElementById('eventoFechaInicio').value = info.dateStr;

                const fechaSiguiente = new Date(fechaSeleccionadaObj);
                fechaSiguiente.setDate(fechaSeleccionadaObj.getDate() + 1);

                const fechaSiguienteStr = fechaSiguiente.toISOString().split('T')[0];
                document.getElementById('eventoFechaFinal').value = fechaSiguienteStr;
            }
        },
        events: function (info, successCallback, failureCallback) {
            $.ajax({
                url: '/Calendario/MostrarEventos',
                type: 'GET',
                success: function (response) {
                    successCallback(response);
                },
                error: function (xhr, status, error) {
                    console.error('Error al cargar eventos:', error);
                    failureCallback(error);
                }
            });
        },
        eventClick: function (info) {
            var event = info.event;
            if (rolUsuario === 1) {
                $('#editarEventoModal').modal('show');
            } else if (rolUsuario === 2) {
                $('#eventoDetallesModal').modal('show');
                document.getElementById('eventoDetallesTitulo').textContent = info.event.title;
                document.getElementById('eventoDetallesDescripcion').textContent = info.event.extendedProps.descripcion;
                document.getElementById('eventoDetallesFechaInicio').textContent = info.event.start.toLocaleDateString();
                document.getElementById('eventoDetallesFechaFinal').textContent = info.event.end ? info.event.end.toLocaleDateString() : 'No especificada';
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Acceso denegado",
                    text: "No tienes permiso para ver este evento."
                });
            }

            $('#eliminarEvento').off('click').on('click', function () {
                eliminarEvento(event);
            });
        }
    });

    calendar.render();

    function eliminarEvento(event) {
        $.ajax({
            url: '/Calendario/EliminarEvento?id=' + event.id,
            type: 'POST',
            contentType: 'application/json',
            success: function (response) {
                console.log('Respuesta del servidor:', response);
                if (response.success) {
                    alert('Evento eliminado correctamente.');
                    var eventToRemove = calendar.getEventById(event.id);
                    eventToRemove.remove();
                    $('#eventoDetallesModal').modal('hide');
                } else {
                    alert('Hubo un error al eliminar el evento: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error al eliminar evento:', error);
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "Algo pasó, intenta de nuevo!"
                });
            }
        });
    }

    // Función para agregar un evento
    function agregarEvento() {
        var nombreEvento = $('#eventoTitulo').val();
        var descripcion = $('#eventoDescripcion').val();
        var fechaInicio = $('#eventoFechaInicio').val();
        var fechaFinal = $('#eventoFechaFinal').val();
        var color = $('#color').val();

        if (!nombreEvento || !fechaInicio || !fechaFinal) {
            Swal.fire({
                icon: 'warning',
                title: 'Campos incompletos',
                text: 'Por favor complete todos los campos.'
            });
            return;
        }

        var eventoData = {
            NOMBRE_EVENTO: nombreEvento,
            DESCRIPCION: descripcion,
            FECHA_INICIO: fechaInicio,
            FECHA_FINAL: fechaFinal,
            COLOR: color
        };

        $.ajax({
            url: '/Calendario/AgregarEvento',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(eventoData),
            success: function (response) {
                if (response.success) {
                    alert('Evento agregado exitosamente.');
                    calendar.addEvent({
                        title: nombreEvento,
                        start: fechaInicio,
                        end: fechaFinal,
                        description: descripcion,
                        color: color
                    });

                    document.getElementById('eventoTitulo').value = "";
                    document.getElementById('eventoDescripcion').value = "";
                    document.getElementById('eventoFechaInicio').value = "";
                    document.getElementById('eventoFechaFinal').value = "";
                    document.getElementById('color').value = "";

                    $('#crearEventoModal').modal('hide');
                } else {
                    alert('Hubo un error al agregar el evento.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error al agregar evento:', error);
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "Algo pasó, intenta de nuevo!"
                });
            }
        });
    }

    // Evento para guardar el evento
    $('#guardarEvento').on('click', function (event) {
        event.preventDefault();
        agregarEvento();
    });
});
