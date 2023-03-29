// JavaScript source code
const addresssGeocodeServiceUrlTemplate = 'https://atlas.microsoft.com/search/address/json?subscription-key={subscription-key}&typeahead=true&api-version=1&query={query}&language={language}&countrySet={countrySet}&view=Auto';
const addresssReverseGeocodeServiceUrlTemplate = 'https://atlas.microsoft.com/search/address/reverse/json?subscription-key={subscription-key}&query={query}&radius={radius}';

var rawData;
var rueBeforeUpdate;
var villeBeforeUpdate;
var adressePositionBeforeUpdate;

function loadFile(event) {
    var image = document.getElementById('output');
    image.style.backgroundImage = "url('" + URL.createObjectURL(event.target.files[0]) + "')";

    readAsBase64(event.target.files[0]);
};

function readAsBase64(file) {
    var reader = new FileReader();
    reader.readAsDataURL(file);

    reader.onload = function () {
        rawData = reader.result;
    };
    reader.onerror = function (error) {
        console.log('Error readAsBase64: ', error);
    };
}

function onImageClick() {
    
    if (appConfig.devLocal) {
        $("#file").click();
    }
    else {
        if (_userIsLogin || _platform) {
            gbGetMedia();
        }
        else {
            $("#file").click();
        }
    }
}

function fileRead(file) {

    var reader = new FileReader();
    reader.onload = function () {
        var binary = reader.result;
        rawData = binary;
    };

    reader.readAsBinaryString(file);
}


function gbDidSuccessGetMedia(data, src) {

    var image = document.getElementById('output');
    image.style.backgroundImage = "url('" + src + "')";
    rawData = data;
}

function gbDidFailGetMedia(errorMessage) {
    console.log('FAIL Call back  gbGetMedia ', errorMessage);
}

function listenRueChange() {
    $("#rue").on('input', function () {
        if (this.value !== '0' && this.value.trim() !== '') {
            $('#ruecheck').hide();
            return;
        }
    });
}

function setAddressWithOriginalValues() {
    var rueValue = $('#rue').val();
    if (rueValue === '0' || rueValue.trim() === '') {
        $('#rue').val(rueBeforeUpdate).trigger('change');
        _currentAdressePosition = adressePositionBeforeUpdate;
    }
}

function clearValues() {
    rawData = null;
    rueBeforeUpdate = null;
    adressePositionBeforeUpdate = null;
    $('#ruecheck').hide();
}

function loadFormOeuvre(feature, e) {
    clearValues();
    _newOeuvre = false;
    _currentAdressePosition =
        JSON.stringify({
            'lat': e.latlng.lat,
            'lon': e.latlng.lng
        });
    adressePositionBeforeUpdate = _currentAdressePosition;

    var data = feature.properties;

    $("#btnSave").css('display', 'none');
    $("#btnCancel").css('display', 'none');
    $("#busy-save-form").css('display', 'none');
    $("#dtCreation").css('display', 'inline-block');

    $("#idOeuvre").attr('value', data.id);

    //Chargement de l'image
    document.getElementById("output").style.backgroundImage = "url('" + data.imageBrUrl + "')";
    $('.output').removeClass("pic-portrait");
    $('.output').removeClass("pic");

    if (data.isLandscape) {
        $('.output').removeClass("pic-portrait");
        $('.output').addClass("pic");
    }
    else {
        $('.output').removeClass("pic");
        $('.output').addClass("pic-portrait");
    }

    //Artiste
    if (data.artiste) {
        $("#artist").attr('value', data.artiste);
    }
    else {
        $("#artist").attr('value', '');
    }

    //Type d'oeuvre
    let textToFind = data.typeOeuvre;
    let dd = document.getElementById('typeOeuvre');
    dd.selectedIndex = [...dd.options].findIndex(option => option.text === textToFind);

    //Adresse
    if (data.rue && data.ville) {
        rueBeforeUpdate = data.rue;
        setAddressWithOriginalValues();
    }

    //Pseudo
    if (data.utlisisateurPseudo) {
        $(".publisher-name").html(data.utlisisateurPseudo);
    }
    else {
        $(".publisher-name").html('Inconnu');
    }

    //Date de publication
    var date = data.date;
    if (date) {
        $(".date-creation").html('le ' + date);
    }

    //Informations complémentaires
    $(".information-complementaire").css('visibility', 'visible');
    if (data.informations) {
        $("#txtinformation").val(data.informations);
    }
    else {
        $("#txtinformation").val('');
    }


    $("#enregistrer").click(function () {
        $("#msgConfirmationForm").css('display', 'inline-block');
        $("#msgConfirmationForm").html('Confirmez vos modifications ?');
        $("#btnSave").css('display', 'inline-block');
        $("#btnCancel").css('display', 'inline-block');
        $("#enregistrer").css('display', 'none');
    });

    $("#btnSave").click(function (e) {
        if (validate(e)) {

            $("#btnSave").css('display', 'none');
            $("#btnCancel").css('display', 'none');
            $("#msgConfirmationForm").html('Traitement en cours ...');
            $("#busy-save-form").css('display', 'inline-block');
            save();
        }
    });

    $("#btnCancel").click(function () {
        $("#btnSave").css('display', 'none');
        $("#btnCancel").css('display', 'none');
        $("#enregistrer").css('display', 'inline-block');
        $("#msgConfirmationForm").css('display', 'none');
    });

    listenRueChange();
}

function loadFormNewOeuvre(position) {

    clearValues();
    _newOeuvre = true;

    $("#btnSave").css('display', 'none');
    $("#btnCancel").css('display', 'none');
    $("#busy-save-form").css('display', 'none');
    $("#dtCreation").css('display', 'none');

    //Chargement de l'image
    document.getElementById("output").style.backgroundImage = "url('./ajouter-photo.png')";
    document.getElementById("output").style.backgroundPosition = "center";
    document.getElementById("output").style.backgroundSize = "contain";
    document.getElementById("output").style.backgroundRepeat = "no-repeat";

    $("#photoIcon").css('display', 'none');

    //Artiste
    $("#artist").attr('value', '');
    console.log('Position on load new oeuvre ', position);
    getAddressWithPosition(position.latlng);
   
    //Pseudo
    if (_userIsLogin) {
        $(".publisher-name").html(_userName);
    }

    //Informations complémentaires
    $(".information-complementaire").css('visibility', 'visible');
    $("#txtinformation").attr('value', '');

    $("#enregistrer").html('Ajouter');
    $("#enregistrer").click(function () {

        $("#msgConfirmationForm").css('display', 'inline-block');
        $("#msgConfirmationForm").html('Partager votre photo ?');
        $("#btnSave").css('display', 'inline-block');
        $("#btnCancel").css('display', 'inline-block');
        $("#enregistrer").css('display', 'none');
    });

    $("#btnSave").click(function (e) {
        if (validate(e)) {
            $("#btnSave").css('display', 'none');
            $("#btnCancel").css('display', 'none');
            $("#msgConfirmationForm").html('Traitement en cours ...');
            $("#busy-save-form").css('display', 'inline-block');
            save();
        }
    });

    $("#btnCancel").click(function () {
        handleBack(false);
    });

    listenRueChange();
}

function addSearchAddress() {
    $("#rue").autocomplete({
        appendTo: "#sidebar",
        minLength: 3,
        delay: 700,
        source: function (request, response) {

            //Create a URL to the Azure Maps search service to perform the address search.
            var requestUrl = addresssGeocodeServiceUrlTemplate.replace('{query}', encodeURIComponent(request.term))
                .replace('{subscription-key}', appConfig.AzureMapSubscriptionKey)
                .replace('{language}', 'fr-FR')
                .replace('{countrySet}', 'FR,GB,US,ES,IT,DE');

            console.log('request map ', requestUrl);

            //Proces the request.
            fetch(requestUrl, {
                method: 'GET',
                mode: 'cors',
                headers: {
                    //'x-ms-client-id': clientId,
                    //'Authorization': 'Bearer ' + token,
                    'Content-Type': 'application/json; charset=utf-8'
                }
            }).then(r => r.json()).then(data => {
                response(data.results);
            });
        },
        select: function (event, ui) {
            event.preventDefault();

            //When a suggestion has been selected.
            var selection = ui.item;
            $("#rue").autocomplete("destroy");

            //Populate the address textbox values.
            _currentAdresse = JSON.stringify(selection.address);
            _currentAdressePosition = JSON.stringify(selection.position);
            document.getElementById('rue').value = selection.address.freeformAddress;

            setAddressWithOriginalValues();

            addSearchAddress();
        }
    }).autocomplete("instance")._renderItem = function (ul, item) {

        //Format the displayed suggestion to show the formatted suggestion string.
        var suggestionLabel = ' ' + item.address.freeformAddress;
        if (item.poi && item.poi.name) {
            suggestionLabel = item.poi.name + ' (' + suggestionLabel + ')';
        }

        return $("<li>")
            .append("<div class='item-line'><div class='fit-picture " + getCountryStyle(item.address.countryCode) + "'></div><a class='street-result'>" + suggestionLabel + "</a></div>")
            .appendTo(ul);
    };
}

function getCountryStyle(countryCode) {
    if (countryCode === "FR") {
        return "fr-country";
    }
    else if (countryCode === "DE") {
        return "de-country";
    }
    else if (countryCode === "GB") {
        return "gb-country";
    }
    else if (countryCode === "ES") {
        return "es-country";
    }
    else if (countryCode === "US") {
        return "us-country";
    }
    else if (countryCode === "IT") {
        return "it-country";
    }
}

function save() {

    var form = $("#oeuvre-form")[0];
    const data = new FormData(form);

    console.log("currentAddress ", _currentAdresse);
    data.append('position', _currentAdressePosition);
    data.append('adresse', _currentAdresse);
    data.append('userId', _userId);
    data.append('userMail', _userEmail);
    data.append('userPseudo', _userName);
    data.append('isAdmin', _isAdmin);

    if (rawData) {
        data.append('rawData', rawData);
    }
    
    $.ajax({
        type: "POST",
        enctype: 'multipart/form-data',
        url: appConfig.UpdateOeuvreInformationFunction,
        data: data,
        processData: false,
        contentType: false,
        cache: false,
        timeout: 600000,
        success: function (data) {

            if (_sidebarFeature && _sidebarFeature.properties) {
                _sidebarFeature.properties = data.properties;
                _sidebarFeature.properties.url = correctImageUrl(data.properties.imageThumbnailUrl);
            }

            $("#msgConfirmationForm").css('display', 'none');
            $("#busy-save-form").css('display', 'none');
            if (_newOeuvre) {
                _lastAddMarker = new L.LatLng(data.geometry.coordinates[0], data.geometry.coordinates[1]);
            }

            handleBack(true);
        },
        error: function (e) {
            console.log("ERROR : ", e);
            $("#busy-save-form").css('display', 'none');
            $("#msgConfirmationForm").html('Connexion interrompue, veuillez réessayer');
            $("#btnSave").css('display', 'inline-block');
            $("#btnCancel").css('display', 'inline-block');
        }
    });
}

function locate(position) {
    locateWithPosition(position.latlng);
}

function locateWithPosition(latlng) {
    var geocodeService = L.esri.Geocoding.geocodeService();
    geocodeService.reverse().latlng(latlng).run(function (error, result) {
        if (!error && result) {
            $('#rue').val(result.address.Address).trigger('change');
        }
    });
}

function getAddressWithPosition(latlng) {
    var requestUrl = addresssReverseGeocodeServiceUrlTemplate.replace('{query}', '' + latlng.lat + ',' + latlng.lng + '')
        .replace('{subscription-key}', appConfig.AzureMapSubscriptionKey)
        .replace('{radius}', '50');
        //A comma seperated string of country codes to limit the suggestions to.

    console.log('request reverse ', requestUrl);

    //Proces the request.
    fetch(requestUrl, {
        method: 'GET',
        mode: 'cors',
        headers: {
            //'x-ms-client-id': clientId,
            //'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json; charset=utf-8'
        }
    })
        .then(r => r.json())
        .then(data => {
            if (data) {
                
                console.log('getAddressWithPosition: result ', data);

                var address = data.addresses[0].address;
                var positionArray = data.addresses[0].position.split(',');

                _currentAdressePosition = JSON.stringify({
                    'lat': positionArray[0],
                    'lon': positionArray[1]
                });

                _currentAdresse = JSON.stringify(address);
                rueBeforeUpdate = address.freeformAddress;
                adressePositionBeforeUpdate = _currentAdressePosition;
                setAddressWithOriginalValues();
            }
        });

}

function validate(e) {

    var rueValue = $('#rue').val();

    if (!rawData  && _newOeuvre) {
        e.preventDefault();
        alert('Veuillez ajouter une photo :)');
        return false;
    }
    else if (rueValue === '0' || rueValue.trim() === '') {
        $('#ruecheck').show();
        return false;
    }

    return true;
}
