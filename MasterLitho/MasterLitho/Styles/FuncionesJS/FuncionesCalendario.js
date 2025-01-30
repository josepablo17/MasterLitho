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

            if (rolUsuario == 2) {
                Swal.fire({
                    icon: "error",
                    title: "Acción no permitida",
                    text: "No tienes permiso para crear eventos."
                });
                return;
            }


            const fechaSeleccionadaObj = new Date(info.dateStr);
            const fechaActual = new Date();
            fechaActual.setHours(0, 0, 0, 0);

            if (fechaSeleccionadaObj < fechaActual) {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "No se puede agendar en una fecha que ya pasó!"
                });
                $('#guardarEvento').prop('disabled', true);
            } else {
                $('#guardarEvento').prop('disabled', false);
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

            $.ajax({
                url: '/Calendario/ObtenerEventByID?id=' + event.id,
                type: 'GET',
                success: function (response) {
                    if (response.success) {
                        
                        if (rolUsuario == 1) {
                            $('#editarEventoId').val(response.data.ID);
                            $('#editarEventoTitulo').val(response.data.NOMBRE_EVENTO);
                            $('#editarEventoDescripcion').val(response.data.DESCRIPCION);
                            $('#editarEventoFechaInicio').val(response.data.FECHA_INICIO.split('T')[0]);
                            $('#editarEventoFechaFinal').val(response.data.FECHA_FINAL ? response.data.FECHA_FINAL.split('T')[0] : '');
                            $('#editarEventoModal').modal('show');


                            $('#eliminarEvento').off('click').on('click', function () {
                                eliminarEvento(event);
                            });
                        } else if (rolUsuario == 2) {
                            $('#eventoDetallesTitulo').text(response.data.NOMBRE_EVENTO);
                            $('#eventoDetallesDescripcion').text(response.data.DESCRIPCION);
                            $('#eventoDetallesFechaInicio').text(response.data.FECHA_INICIO.split('T')[0]);
                            $('#eventoDetallesFechaFinal').text(response.data.FECHA_FINAL ? response.data.FECHA_FINAL.split('T')[0] : 'N/A');
                            $('#eventoDetallesModal').modal('show');
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Acceso denegado",
                                text: "No tienes permiso para ver este evento."
                            });
                        }

                    } else {
                        Swal.fire({
                            icon: "error",
                            title: "Acceso denegado",
                            text: "No tienes permiso para ver este evento."
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error al obtener el evento:', error);
                    Swal.fire({
                        icon: "error",
                        title: "Oops...",
                        text: "No se pudo cargar el evento, intenta nuevamente."
                    });
                }
            });
        }
    });
    
    calendar.render();

    

    $(document).ready(function () {
        $('.eventoInvitados').select2({
            language: "es",
            ajax: {
                url: '/Calendario/ObtenerCorreosSeleccionados',
                type: 'GET',
                dataType: 'json',
                delay: 250,  
                data: function (params) {
                    return {
                        q: params.term  
                    };
                },
                processResults: function (data) {
                    
                    return {
                        results: data.results
                    };
                }
            },
            minimumInputLength: 0 
        });
    });

    $('.eventoInvitados').on('change', function () {
        var correosSeleccionados = $(this).val();
        console.log("Correos seleccionados:", correosSeleccionados);
    });




    function editarEvento() {
        var eventoId = $('#editarEventoId').val();
        var titulo = $('#editarEventoTitulo').val();
        var descripcion = $('#editarEventoDescripcion').val();
        var fechaInicio = $('#editarEventoFechaInicio').val();
        var fechaFinal = $('#editarEventoFechaFinal').val();
        var color = $('#editarEventoColor').val();

        var inicio = new Date(fechaInicio);
        var final = new Date(fechaFinal);

        if (inicio > final) {
            Swal.fire({
                icon: "error",
                title: "No se puede agendar, esas fechas no son válidas"
            });
            return;
        }
        var eventoData = {
            ID_EVENTO: eventoId,
            NOMBRE_EVENTO: titulo,
            DESCRIPCION: descripcion,
            FECHA_INICIO: fechaInicio,
            FECHA_FINAL: fechaFinal || null,
            COLOR: color
        };

        $.ajax({
            url: '/Calendario/EditarEvento',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(eventoData),
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Evento editado exitosamente"
                    });

                    var eventToUpdate = calendar.getEventById(eventoId);
                    eventToUpdate.setProp('title', titulo);
                    eventToUpdate.setProp('start', fechaInicio);
                    eventToUpdate.setProp('end', fechaFinal || null);
                    eventToUpdate.setExtendedProp('descripcion', descripcion);



                    $('#editarEventoModal').modal('hide');
                    location.reload();

                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error al actualizar",
                        text: "Ha ocurrido un error al actualizar, intenta de nuevo"
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error('Error al actualizar evento:', error);
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "No se pudo actualizar el evento, intenta nuevamente."
                });
            }
        });
    }

    $('#editarEvento').on('click', function (event) {
        event.preventDefault();
        editarEvento();
    });



    function eliminarEvento(event) {
        $.ajax({
            url: '/Calendario/EliminarEvento?id=' + event.id,
            type: 'POST',
            contentType: 'application/json',
            success: function (response) {
                console.log('Respuesta del servidor:', response);
                if (response.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Evento eliminado exitosamente"
                    });
                    var eventToRemove = calendar.getEventById(event.id);
                    eventToRemove.remove();
                    $('#editarEventoModal').modal('hide');
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

    

    function agregarEvento(selectedUser) {
        var correosSeleccionados = $('.eventoInvitados').val();
        console.log("Correos seleccionados (en JS):", correosSeleccionados);

        
        if (!Array.isArray(correosSeleccionados)) {
            correosSeleccionados = [correosSeleccionados];  
        }

        if (correosSeleccionados.length > 0) {
            correosSeleccionados = correosSeleccionados.join(','); 
            console.log("Correos seleccionados (después de join):", correosSeleccionados);
        }

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

        var inicio = new Date(fechaInicio);
        var final = new Date(fechaFinal);
        if (inicio > final) {
            Swal.fire({
                icon: "error",
                title: "No se puede agendar, esas fechas no son válidas"
            });
            return;
        }

        

        var eventoData = {
            NOMBRE_EVENTO: nombreEvento,
            DESCRIPCION: descripcion,
            FECHA_INICIO: fechaInicio,
            FECHA_FINAL: fechaFinal,
            COLOR: color,
            invitados: correosSeleccionados
        };


        console.log("Datos que se envían al backend:", eventoData);
        const swalLoading = Swal.fire({
            title: 'Cargando...',
            text: 'Por favor espera mientras se agrega el evento.',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });


        $.ajax({
            url: '/Calendario/AgregarEvento',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(eventoData),
            success: function (response) {
                swalLoading.close();

                if (response.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Evento agregado exitosamente"
                    });


                    calendar.addEvent({
                        title: nombreEvento,
                        start: fechaInicio,
                        end: fechaFinal,
                        description: descripcion,
                        color: color,
                        invitados: correosSeleccionados
                    });


                    $('#eventoTitulo').val('');
                    $('#eventoDescripcion').val('');
                    $('#eventoFechaInicio').val('');
                    $('#eventoFechaFinal').val('');
                    $('#color').val('');
                    $('#eventoInvitados').val(null).trigger('change');


                    $('#crearEventoModal').modal('hide');
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error al agregar el evento",
                        text: "Hubo un error al agregar el evento, intenta de nuevo."
                    });
                }
            },
            error: function (xhr, status, error) {
                swalLoading.close();
                console.error('Error al agregar evento:', error);
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "Algo pasó, intenta de nuevo!"
                });
            }
        });
    }
    $('#guardarEvento').on('click', function (event) {
        event.preventDefault();
        agregarEvento();
    });
    

   
    
    

});

    

    



