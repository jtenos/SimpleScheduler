import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-schedules',
  templateUrl: './schedules.component.html',
  styleUrls: ['./schedules.component.scss']
})
export class SchedulesComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
/* EDIT SCHEDULE

jQuery(() => {

    const errorMessage = jQuery("[name=ErrorMessage]").val() as string;
    if (errorMessage) {
        bootbox.alert(errorMessage);
    }

    (function setRecurTime() {
        const recurTimeText = jQuery("[name=Schedule\\.RecurTime]").val() as string;
        if (!recurTimeText || recurTimeText === "00:00" || recurTimeText === "00:00:00") { return; }

        const timeFields = recurTimeText.split(":");
        const numHours = parseInt(timeFields[0], 10);
        let numMinutes = parseInt(timeFields[1], 10);
        if (numMinutes) {
            numMinutes = 60 * numHours + numMinutes;
            jQuery("#recur-time-minutes").val(numMinutes.toString());
        } else {
            jQuery("#recur-time-hours").val(numHours.toString());
        }
    })();

    function getTimeOfDayLabel(utcTime: string, callback: (s: string) => void) {
        if (!utcTime) { callback(""); return; }
        const $form = jQuery("#get-server-time-form");
        $form.find("[name=UtcTime]").val(utcTime);
        jQuery.ajax({
            type: $form.attr("method"),
            url: $form.attr("action"),
            data: $form.serialize(),
            dataType: "text"
        }).done(function (result: string) {
            callback(result);
        });
    }

    function setUpServerTime($time: JQuery, $span: JQuery) {
        function go() {
            getTimeOfDayLabel($time.val() as string, (result: string) => {
                jQuery($span).text(result);
            });
        }
        jQuery($time).change(() => go());
        go();
    }

    setUpServerTime(jQuery("[name=Schedule\\.TimeOfDayUTC]"), jQuery("#time-of-day-server"));
    setUpServerTime(jQuery("[name=Schedule\\.RecurBetweenStartUTC]"), jQuery("#recur-start-time-server"));
    setUpServerTime(jQuery("[name=Schedule\\.RecurBetweenEndUTC]"), jQuery("#recur-end-time-server"));

    jQuery("#recur-time-hours").change(function () {
        jQuery("#recur-time-minutes").val("");
        let val = "";
        if (jQuery(this).val()) {
            val = jQuery(this).val() as string;
            while (val.length < 2) {
                val = `0${val}`;
            }
            val += ":00";
        }
        jQuery("[name=Schedule\\.RecurTime]").val(val);
    });
    jQuery("#recur-time-minutes").change(function () {
        jQuery("#recur-time-hours").val("");
        let val = "";
        const totalMinutes = parseInt(jQuery(this).val() as string, 10);
        if (jQuery(this).val()) {
            let numHours = Math.floor(totalMinutes / 60).toString();
            let numMinutes = (totalMinutes % 60).toString();
            while (numHours.length < 2) { numHours = `0${numHours}`; }
            while (numMinutes.length < 2) { numMinutes = `0${numMinutes}`; }
            val = `${numHours}:${numMinutes}`;
        }
        jQuery("[name=Schedule\\.RecurTime]").val(val);
    });
});
*/

/*





*/