function inicializarDataTable(selector, opcionesAdicionales = {}) {
    // Configuración base común para todos los DataTables
    const configuracionBase = {
        paging: true,
        searching: true,
        ordering: true,
        responsive: true,
        dom: 'frtip',
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.5/i18n/es-ES.json"
        },
        createdRow: function (row, data, dataIndex) {
            $(row).css('background-color', '#2c2f33'); // Fondo oscuro para filas
            $(row).css('color', '#f5f5f5'); // Texto claro
        }
    };

    // Combinar la configuración base con las opciones adicionales (sobrescribe si es necesario)
    const configuracionFinal = $.extend({}, configuracionBase, opcionesAdicionales);

    // Inicializar el DataTable con la configuración final
    $(selector).DataTable(configuracionFinal);
}
