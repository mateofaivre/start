L.Control.Topbar = L.Control.extend({

    initialize: function (placeholder, options) {
        L.setOptions(this, options);

        // Find content container
        var content = this._contentContainer = L.DomUtil.get(placeholder);

        // Remove the content container from its original parent
        content.parentNode.removeChild(content);

        var l = 'leaflet-';

        // Create top bar container
        var container = this._container =
            L.DomUtil.create('div', l + 'topbar ' + this.options.position);

        // Style and attach content container
        L.DomUtil.addClass(content, l + 'control');
        container.appendChild(content);
    },

    addTo: function (map) {
        var container = this._container;
        var content = this._contentContainer;
        // Attach sidebar container to controls container
        var controlContainer = map._controlContainer;
        controlContainer.insertBefore(container, controlContainer.firstChild);

        this._map = map;

        // Make sure we don't drag the map when we interact with the content
        var stop = L.DomEvent.stopPropagation;
        var fakeStop = L.DomEvent._fakeStop || stop;
        L.DomEvent
            .on(content, 'contextmenu', stop)
            .on(content, 'click', fakeStop)
            .on(content, 'mousedown', stop)
            .on(content, 'touchstart', stop)
            .on(content, 'dblclick', fakeStop)
            .on(content, 'mousewheel', stop)
            .on(content, 'MozMousePixelScroll', stop);


        return this;
    },

    onRemove: function (map) {
        // Nothing to do here
    }
});

L.control.topbar = function (opts) {
    return new L.Control.Topbar(opts);
}
