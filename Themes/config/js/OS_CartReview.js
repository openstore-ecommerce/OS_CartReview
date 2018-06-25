
$(document).ready(function () {

    // get list of records via ajax:  NBrightRazorTemplate_nbxget({command}, {div of data passed to server}, {return html to this div} )
    nbxget('os_cartreview_getdata', '#selectparams', '#editdata');

    $('.actionbuttonwrapper #cmdreturn').unbind('click');
    $('.actionbuttonwrapper #cmdreturn').click(function () {
        $('#selecteditemid').val(''); // clear sleecteditemid.        
        nbxget('os_cartreview_getdata', '#selectparams', '#editdata');
    });

    $('.actionbuttonwrapper #cmddelete').unbind('click');
    $('.actionbuttonwrapper #cmddelete').click(function () {
        if (confirm($('#deletemsg').val())) {
            nbxget('os_cartreview_deleterecord', '#editdata');
        }
    });

    $('.selecteditlanguage').unbind('click');
    $('.selecteditlanguage').click(function () {
        $('#editlang').val($(this).attr('lang')); // alter lang after, so we get correct data record
        nbxget('os_cartreview_getdata', '#selectparams', '#editdata');
    });


});

$(document).on("nbxgetcompleted", nbxgetCompleted); // assign a completed event for the ajax calls


function nbxgetCompleted(e) {

    if (e.cmd == 'os_cartreview_addnew') {
        $('#newitem').val(''); // clear item so if new was just created we don;t create another record
        $('#selecteditemid').val($('#itemid').val()); // move the itemid into the selecteditemid, so page knows what itemid is being edited
        OS_CartReview_DetailButtons();
        $('.processing').hide(); 
    }

    if (e.cmd == 'os_cartreview_deleterecord') {
        $('#selecteditemid').val(''); // clear selecteditemid, it now doesn;t exists.
        nbxget('os_cartreview_getdata', '#selectparams', '#editdata');// relist after delete
    }

    if (e.cmd == 'os_cartreview_getdata') {
        $('.processing').hide();

        $('#OS_CartReview_cmdSearch').unbind("click");
        $('#OS_CartReview_cmdSearch').click(function () {
            $('.processing').show();
            $('#pagenumber').val('1');
            $('#searchtext').val($('#OrderAdmin_searchtext').val());
            nbxget('os_cartreview_getdata', '#selectparams', '#editdata');
        });

        $('#OS_CartReview_cmdReset').unbind("click");
        $('#OS_CartReview_cmdReset').click(function () {
            $('.processing').show();
            $('#pagenumber').val('1');
            $('#searchtext').val('');
            nbxget('os_cartreview_getdata', '#selectparams', '#editdata');
        });

    }

    // check if we are displaying a list or the detail and do processing.
    if (($('#selecteditemid').val() != '') || (e.cmd == 'os_cartreview_addnew')) {
        // PROCESS DETAIL
        OS_CartReview_DetailButtons();

        $('.processing').hide(); 

    } else {
        //PROCESS LIST
        OS_CartReview_ListButtons();
        $('.edititem').unbind('click');
        $('.edititem').click(function () {
            $('.processing').show();
            $('#selecteditemid').val($(this).attr("itemid")); // assign the sleected itemid, so the server knows what item is being edited
            nbxget('os_cartreview_getdata', '#selectparams', '#editdata'); // do ajax call to get edit form
        });
        $('.processing').hide(); 
    }



}

function OS_CartReview_DetailButtons() {
    $('#cmddelete').show();
    $('#cmdreturn').show();
    $('#addnew').hide();
    $('input[datatype="date"]').datepicker(); // assign datepicker to any ajax loaded fields
}

function OS_CartReview_ListButtons() {
    $('#cmddelete').hide();
    $('#cmdreturn').hide();
    $('#addnew').show();
}


