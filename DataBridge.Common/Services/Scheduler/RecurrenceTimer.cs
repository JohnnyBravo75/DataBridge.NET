using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class RecurrenceGenerator
    {
        /// <summary>
        /// Checks the schedule.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="numberOfNextValues">The number of needed next values.</param>
        /// <param name="startDate">Optional a start date.</param>
        /// <param name="toDate">Optional a end date.</param>
        /// <returns>
        /// the next dates
        /// </returns>
        public IList<DateTime> GenerateRecurrences(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            /*
            ||Beschreibung:
            ||Serienmustergenerator. Generiert aus einer Seriendefinition/muster die Datumswerte.
            ||
            ||erstellt: 13.11.2008 tl
            ||
            ||
            ||===========================================================================
            ||RecurrenceTyp    Eigenschaften      Beispiel
            ||===========================================================================
            ||DAILY      Interval           Alle N Tage
            ||           DayOfWeekMask      Jeden Dienstag, Mittwoch und Donnerstag
            ||
            ||WEEKLY     Interval           Alle N Wochen
            ||           DayOfWeekMask      Jeden Dienstag, Mittwoch und Donnerstag
            ||
            ||MONTHLY    Interval           Alle N Monate
            ||           DayOfMonth         Der N-te Tag im Monat
            ||
            ||MONTHLYnTH Interval           Alle N Monate
            ||           Instance           Der N-te Dienstag
            ||           DayOfWeekMask      Jeden Dienstag und Mittwoch
            ||
            ||YEARLY     Interval           Alle N Jahre
            ||           DayOfMonth         Der N-te Tag im Monat
            ||           MonthOfYear        Februar
            ||
            ||YEARLYnTH  Interval           Alle N Jahre
            ||           Instance           Der N-te Dienstag
            ||           MonthOfYear        Februar
            ||           DayOfWeekMask      Dienstag, Mittwoch, Donnerstag
            ||
            ||===========================================================================
            ||Legende:
            ||Instance         1-4(5)
            ||DayOfWeekMask    z.B. MDMDF--
            ||DayOfMonth       1-31
            ||MonthOfYear      1-12
            ||StartDate        Startdatum der Defintion
            ||EndDate          Datum oder...
            ||Occurrences      ...Anzahl von Wiederholungen
            ||Interval		  1-...   All N days/weeks,.. when the event recurrence.
            ||===========================================================================
            ||
            */

            IList<DateTime> executeDates = new List<DateTime>();

            /************täglich****************/
            if (schedule.Type == RecurrencePattern.Types.DAILY)
            {
                executeDates = this.GenerateDaily(schedule, numberOfNextValues, startDate, toDate);
            }

            /************wöchentlich****************/
            if (schedule.Type == RecurrencePattern.Types.WEEKLY)
            {
                executeDates = this.GenerateWeekly(schedule, numberOfNextValues, startDate, toDate);
            }

            /************monatlich****************/
            if (schedule.Type == RecurrencePattern.Types.MONTHLY)
            {
                executeDates = this.GenerateMonthly(schedule, numberOfNextValues, startDate, toDate);
            }

            /************monatlich****************/
            if (schedule.Type == RecurrencePattern.Types.MONTHLYnTH)
            {
                executeDates = this.GenerateMonthlyNth(schedule, numberOfNextValues, startDate, toDate);
            }

            /************jährlich***************/
            if (schedule.Type == RecurrencePattern.Types.YEARLY)
            {
                executeDates = this.GenerateYearly(schedule, numberOfNextValues, startDate, toDate);
            }

            /************jährlich***************/
            if (schedule.Type == RecurrencePattern.Types.YEARLYnTH)
            {
                executeDates = this.GenerateYearlyNth(schedule, numberOfNextValues, startDate, toDate);
            }

            /************Interval***************/
            if (schedule.Type == RecurrencePattern.Types.INTERVAL)
            {
                executeDates = this.GenerateInterval(schedule, numberOfNextValues, startDate, toDate);
            }

            return executeDates;
        }

        private IList<DateTime> GenerateInterval(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            //long countInstance = 1;
            //DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            //int maxEndYearly = 50;
            //int maxEndMonthly = 100;
            //int maxEndWeekly = 2000;
            int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************Interval***************/

            if (schedule.Interval != null && schedule.Interval != 0)
            {
                while (true)
                {
                    executeDates.Add((currentDate.Add(startTime)));
                    counter += 1;

                    if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                    {
                        break;
                    }

                    // N Tage hinzu
                    currentDate = currentDate.AddDays(schedule.Interval.Value);

                    if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndDaily))
                    {
                        break;
                    }

                    countLoop += 1;
                }
            }

            return executeDates;
        }

        private IList<DateTime> GenerateYearlyNth(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            long countInstance = 1;
            DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            int maxEndYearly = 50;
            //int maxEndMonthly = 100;
            //int maxEndWeekly = 2000;
            //int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************jährlich***************/

            //Monatanfang des Monats ermitteln (es muss hier immer ab Monatsanfang durchgelaufen werden)
            monthBeginDate = new DateTime(currentDate.Year, schedule.MonthOfYear.Value, 1);
            currentDate = monthBeginDate;

            //Instanzcounter für den N.ten im Monat
            countInstance = 1;

            while (true)
            {
                //schauen, ob dieser in der TagesMaske ist
                if (this.DayOfWeekMaskFits(currentDate, schedule.DayOfWeekMask))
                {
                    //Wenn das N.-Mal zutrifft und dies in der Instanznr. so vermerkt ist, dann...
                    if (countInstance == schedule.DayInstance && currentDate >= schedule.From.Date)
                    {
                        //	INSERT INTO	mis.tb_appointment_recurrence_date(aprd_apre_id,aprd_date) VALUES(i_nApre_Id,v_CurrDate+v_nStartTime);
                        executeDates.Add(currentDate.Add(startTime));
                        counter += 1;

                        if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                        {
                            break;
                        }
                    }

                    countInstance += 1;
                }

                // 1 Tag hinzu
                currentDate = currentDate.AddDays(1);

                //Wenn Monat vorbei und der nächste erreicht ist,...
                if (currentDate == monthBeginDate.AddMonths(1))
                {
                    // auf Monatsanfang im nächsten Jahr setzen
                    currentDate = monthBeginDate.AddYears(1);

                    //evtl. noch N Jahre hinzuzählen
                    currentDate = currentDate.AddYears((int)schedule.Interval.Value - 1);
                    monthBeginDate = currentDate;

                    //neues Jahr, d.h. Instanzcounter zurücksetzen
                    counter += 1;
                }

                if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndYearly))
                {
                    break;
                }

                countLoop += 1;
            }

            return executeDates;
        }

        private IList<DateTime> GenerateYearly(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            //long countInstance = 1;
            //DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            int maxEndYearly = 50;
            //int maxEndMonthly = 100;
            //int maxEndWeekly = 2000;
            //int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************jährlich***************/

            while (true)
            {
                //schauen ob der aktuelle Tag, und Monat passt...
                DateTime date = new DateTime(currentDate.Year, schedule.MonthOfYear.Value, schedule.DayOfMonth.Value);

                if (currentDate == date)
                {
                    executeDates.Add(currentDate.Add(startTime));
                    counter += 1;

                    if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                    {
                        break;
                    }
                }

                // 1 Tag hinzu
                currentDate = currentDate.AddDays(1);

                //wenn Jahresanfang, evtl. noch N Jahre hinzuzählen
                if (currentDate == currentDate.FirstDayOfYear())
                {
                    currentDate = currentDate.AddYears((int)schedule.Interval.Value - 1); // ADD_YEARS(v_CurrDate, schedul.apre_interval-1);
                }

                if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndYearly))
                {
                    break;
                }

                countLoop += 1;
            }

            return executeDates;
        }

        private IList<DateTime> GenerateMonthlyNth(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            long countInstance = 1;
            //DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            //int maxEndYearly = 50;
            int maxEndMonthly = 100;
            //int maxEndWeekly = 2000;
            //int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************monatlich****************/

            //aktuellen Monatanfang ermitteln (es muss hier immer ab Monatsanfang durchgelaufen werden)
            currentDate = currentDate.FirstDayOfMonth();

            //Instanzcounter für den N.ten im Monat
            countInstance = 1;

            while (true)
            {
                //schauen, ob dieser in der TagesMaske ist
                if (this.DayOfWeekMaskFits(currentDate, schedule.DayOfWeekMask))
                {
                    //Wenn das N.-Mal zutrifft und dies in der Instanznr. so vermerkt ist, dann...
                    if (countInstance == schedule.DayInstance && currentDate >= schedule.From.Date)
                    {
                        executeDates.Add(currentDate.Add(startTime));
                        counter += 1;

                        if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                        {
                            break;
                        }
                    }

                    countInstance += 1;
                }

                // 1 Tag hinzu
                currentDate = currentDate.AddDays(1);

                //wenn Monatsanfang, evtl. noch N Monate hinzuzählen
                if (currentDate == currentDate.FirstDayOfMonth())
                {
                    currentDate = currentDate.AddMonths((int)schedule.Interval.Value - 1);

                    //neuer Monat, d.h. Instanzcounter zurücksetzen
                    countInstance = 1;
                }

                if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndMonthly))
                {
                    break;
                }

                countLoop += 1;
            }

            return executeDates;
        }

        private IList<DateTime> GenerateMonthly(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            //long countInstance = 1;
            //DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            //int maxEndYearly = 50;
            int maxEndMonthly = 100;
            //int maxEndWeekly = 2000;
            //int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************monatlich****************/

            while (true)
            {
                //schauen ob der aktuelle Tag, z.B. der 12. gleich dem Tag 12. im Monat...
                if (currentDate.Day == schedule.DayOfMonth)
                {
                    executeDates.Add(currentDate.Add(startTime));
                    counter += 1;

                    if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                    {
                        break;
                    }
                }

                // 1 Tag hinzu
                currentDate = currentDate.AddDays(1);

                //wenn Monatsanfang, evtl. noch N Monate hinzuzählen
                if (currentDate == currentDate.FirstDayOfMonth())
                {
                    currentDate = currentDate.AddMonths((int)schedule.Interval.Value - 1);// ADD_MONTHS(v_CurrDate, schedul.apre_interval-1);
                }

                if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndMonthly))
                {
                    break;
                }

                countLoop += 1;
            }

            return executeDates;
        }

        private IList<DateTime> GenerateWeekly(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            //long countInstance = 1;
            //DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            //int maxEndYearly = 50;
            //int maxEndMonthly = 100;
            int maxEndWeekly = 2000;
            //int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************wöchentlich****************/

            while (true)
            {
                //schauen, ob dieser in der TagesMaske ist
                if (this.DayOfWeekMaskFits(currentDate, schedule.DayOfWeekMask))
                {
                    executeDates.Add(currentDate.Add(startTime));
                    counter += 1;

                    if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                    {
                        break;
                    }
                }

                // N Tage hinzu
                currentDate = currentDate.AddDays(1);

                //wenn Wochenanfang, evtl. noch N Wochen hinzuzählen
                if (currentDate == currentDate.FirstDayOfWeek())
                {
                    currentDate = currentDate.AddDays((schedule.Interval.Value - 1) * 7);
                }

                if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndWeekly))
                {
                    break;
                }

                countLoop += 1;
            }

            return executeDates;
        }

        private IList<DateTime> GenerateDaily(RecurrencePattern schedule, int? numberOfNextValues, DateTime? startDate = null, DateTime? toDate = null)
        {
            DateTime currentDate;
            long counter = 0;
            long countLoop = 0;
            //long countInstance = 1;
            //DateTime monthBeginDate;
            TimeSpan startTime;
            long maxLoopCount = 20000000;

            //int maxEndYearly = 50;
            //int maxEndMonthly = 100;
            //int maxEndWeekly = 2000;
            int maxEndDaily = 2000000;

            currentDate = (startDate != null) ? startDate.Value : schedule.From;

            DateTime firstDate = currentDate;

            startTime = schedule.From.TimeOfDay;// (currentDate - currentDate.Date);

            currentDate = currentDate.Date;

            IList<DateTime> executeDates = new List<DateTime>();

            /************täglich****************/

            if (schedule.Interval != null && schedule.Interval != 0)
            {
                while (true)
                {
                    executeDates.Add(currentDate.Add(startTime));
                    counter += 1;

                    if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                    {
                        break;
                    }

                    // N Tage hinzu
                    currentDate = currentDate.AddDays(schedule.Interval.Value);

                    if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndDaily))
                    {
                        break;
                    }

                    countLoop += 1;
                }
            }
            else if (!string.IsNullOrEmpty(schedule.DayOfWeekMask))
            {
                while (true)
                {
                    //schauen, ob dieser in der TagesMaske ist
                    if (this.DayOfWeekMaskFits(currentDate, schedule.DayOfWeekMask))
                    {
                        executeDates.Add(currentDate.Add(startTime));
                        counter += 1;

                        if (this.CheckBreak(numberOfNextValues, toDate, firstDate, executeDates))
                        {
                            break;
                        }
                    }

                    //1 Tag hinzu
                    currentDate = currentDate.AddDays(1);

                    if ((counter >= schedule.Occurrences) || (currentDate > schedule.To) || (countLoop >= maxLoopCount) || (schedule.To == null && schedule.Occurrences == null && counter >= maxEndDaily))
                    {
                        break;
                    }

                    countLoop += 1;
                }
            }

            return executeDates;
        }

        /// <summary>
        /// Checks the break.
        /// </summary>
        /// <param name="nextValues">The next values.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="firstDate">The first date.</param>
        /// <param name="executeDates">The execute dates.</param>
        /// <returns></returns>
        private bool CheckBreak(int? numberOfNextValues, DateTime? toDate, DateTime firstDate, IList<DateTime> executeDates)
        {
            return ((numberOfNextValues != null && executeDates.Count >= numberOfNextValues.Value) && (executeDates.Last() > firstDate)) ||
                    (toDate != null && executeDates.Last() >= toDate.Value);
        }

        /// <summary>
        /// Checks if a day of week mask fits to a date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="dayOfWeekMask">The day of week mask. "MDMDFSS"</param>
        /// <returns></returns>
        private bool DayOfWeekMaskFits(DateTime date, string dayOfWeekMask)
        {
            /*
            ||Beschreibung:
            ||überprüft, ob ein Datum zu einer Wochenmaske (z.B. MDMDF--) passt.
            ||
            ||erstellt: 13.11.2008 tl
            ||
            */
            DayOfWeek dayOfWeek;
            bool result = false;

            if (string.IsNullOrEmpty(dayOfWeekMask))
            {
                return result;
            }

            dayOfWeek = date.DayOfWeek;

            if (dayOfWeek == DayOfWeek.Monday && dayOfWeekMask.Substring(0, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(0, 1))
                result = true;
            else if (dayOfWeek == DayOfWeek.Tuesday && dayOfWeekMask.Substring(1, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(1, 1))
                result = true;
            else if (dayOfWeek == DayOfWeek.Wednesday && dayOfWeekMask.Substring(2, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(2, 1))
                result = true;
            else if (dayOfWeek == DayOfWeek.Thursday && dayOfWeekMask.Substring(3, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(3, 1))
                result = true;
            else if (dayOfWeek == DayOfWeek.Friday && dayOfWeekMask.Substring(4, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(4, 1))
                result = true;
            else if (dayOfWeek == DayOfWeek.Saturday && dayOfWeekMask.Substring(5, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(5, 1))
                result = true;
            else if (dayOfWeek == DayOfWeek.Sunday && dayOfWeekMask.Substring(6, 1) == RecurrencePattern.FullDayOfWeekMask.Substring(6, 1))
                result = true;

            return result;
        }
    }

    public class RecurrencePattern
    {
        public enum Types
        {
            DAILY,
            WEEKLY,
            MONTHLY,
            MONTHLYnTH,
            YEARLY,
            YEARLYnTH,
            INTERVAL
        }

        public const string EmptyDayOfWeekMask = "-----";
        public const string FullDayOfWeekMask = "MDMDFSS";
        public const string WorkingDayOfWeekMask = "MDMDF--";

        private RecurrencePattern.Types type;
        private string dayOfWeekMask = EmptyDayOfWeekMask;

        private TimeSpan? toTime;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets das Startdatum
        /// </summary>
        public virtual DateTime From { get; set; }

        /// <summary>
        /// Gets or sets das EndeDatum
        /// </summary>
        public virtual DateTime? To { get; set; }

        /// <summary>
        /// Gets or sets the Type (DAILY, WEEKLY, MONTHLY, MONTHLYnTH, YEARLY, YEARLYnTH)
        /// </summary>
        public virtual RecurrencePattern.Types Type
        {
            get
            {
                return this.type;
            }
            set
            {
                if (value != null)
                {
                    if (this.type != null && this.type != value)
                    {
                        this.dayOfWeekMask = EmptyDayOfWeekMask;
                        //this.RaisePropertyChanged("YdayOfWeekMask");
                        //this.RaisePropertyChanged("TdayOfWeekMask");
                        //this.RaisePropertyChanged("MdayOfWeekMask");
                        //this.RaisePropertyChanged("Montag");
                        //this.RaisePropertyChanged("Dienstag");
                        //this.RaisePropertyChanged("Mittwoch");
                        //this.RaisePropertyChanged("Donnerstag");
                        //this.RaisePropertyChanged("Freitag");
                        //this.RaisePropertyChanged("Samstag");
                        //this.RaisePropertyChanged("Sonntag");
                    }

                    if (this.type != value)
                    {
                        this.type = value;

                        if (this.type != null)
                        {
                            this.Reset();
                        }
                    }
                }
            }
        }

        private void Reset()
        {
            // Sonderfall, wenn von Interval auf eine anderer Eintrag gewechselt wurde, dann kann im Intervall ein Kommawert stehen.
            if (this.type != RecurrencePattern.Types.INTERVAL && this.Interval > 0 && this.Interval < 1)
            {
                this.Interval = 1;
                //this.RaisePropertyChanged("DInterval");
                //this.RaisePropertyChanged("MInterval");
                //this.RaisePropertyChanged("WInterval");
            }

            if (this.type == RecurrencePattern.Types.MONTHLY)
            {
                this.dayOfWeekMask = null;
                this.DayInstance = null;
                this.MonthOfYear = null;
            }
            else if (this.type == RecurrencePattern.Types.YEARLY)
            {
                this.dayOfWeekMask = null;
                this.DayInstance = null;
            }
            else if (this.dayOfWeekMask == null)
            {
                this.dayOfWeekMask = EmptyDayOfWeekMask;
            }

            if (this.type == RecurrencePattern.Types.WEEKLY)
            {
                this.DayOfMonth = null;
                this.DayInstance = null;
                this.MonthOfYear = null;
            }

            if (this.type == RecurrencePattern.Types.DAILY)
            {
                this.DayOfMonth = null;
                this.DayInstance = null;
                this.MonthOfYear = null;
            }

            if (this.type == RecurrencePattern.Types.YEARLYnTH)
            {
                this.DayOfMonth = null;
            }

            if (this.type == RecurrencePattern.Types.MONTHLYnTH)
            {
                this.MonthOfYear = null;
                this.DayOfMonth = null;
            }

            if (this.type == RecurrencePattern.Types.INTERVAL)
            {
                this.DayOfMonth = null;
                this.DayInstance = null;
                this.MonthOfYear = null;
            }
        }

        /// <summary>
        /// Gets or sets the Interval 1-...   All N days/weeks,,.. when the event recurrence.
        /// </summary>
        public virtual double? Interval { get; set; }

        /// <summary>
        /// Gets or sets the DayOfWeekMask 'MDMDF--'
        /// </summary>
        public virtual string DayOfWeekMask
        {
            get
            {
                return this.dayOfWeekMask;
            }
            set
            {
                this.dayOfWeekMask = value;
            }
        }

        /// <summary>
        /// Gets or sets the DayOfMonth (1-31)
        /// </summary>
        public virtual int? DayOfMonth { get; set; }

        /// <summary>
        /// Gets or sets the DayInstance
        /// </summary>
        public virtual int? DayInstance { get; set; }

        /// <summary>
        /// Gets or sets the MonthOfYear (1-12)
        /// </summary>
        public virtual int? MonthOfYear { get; set; }

        /// <summary>
        /// Gets or sets the Occurrences
        /// </summary>
        public virtual int? Occurrences { get; set; }

        /// <summary>
        /// Gets or sets the Duration (1..)
        /// </summary>
        public virtual decimal? DurationDays { get; set; }

        /// <summary>
        /// Determines whether [is on working days].
        /// </summary>
        /// <returns></returns>
        private bool IsOnWorkingDays()
        {
            if (this.DayOfWeekMask == WorkingDayOfWeekMask)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Führt die Validierung durch
        /// </summary>
        /// <returns>the Validation message</returns>
        public string Validate()
        {
            string errorMessage = "";

            if (this.Type == RecurrencePattern.Types.DAILY)
            {
                errorMessage = this.ValidateDaily();
            }

            if (this.Type == RecurrencePattern.Types.WEEKLY)
            {
                errorMessage = this.ValidateWeekly();
            }

            if (this.Type == RecurrencePattern.Types.MONTHLYnTH)
            {
                errorMessage = this.ValidateMonthlyNth();
            }

            if (this.Type == RecurrencePattern.Types.MONTHLY)
            {
                errorMessage = this.ValidateMonthly();
            }

            if (this.Type == RecurrencePattern.Types.YEARLY)
            {
                errorMessage = this.ValidateYearly();
            }

            if (this.Type == RecurrencePattern.Types.YEARLYnTH)
            {
                errorMessage = this.ValidateYearlyNth();
            }

            if (this.Type == RecurrencePattern.Types.INTERVAL)
            {
                errorMessage = this.ValidateInterval();
            }

            if (this.From > this.To + this.ToTime)
            {
                errorMessage = "Der Serienzeitraum ist ungültig, das Beginndatum darf nicht grösser als das Enddatum sein.";
            }

            if (this.Interval < 0)
            {
                errorMessage = "Das Serienmuster ist ungültig, bitte geben Sie eine Häufigkeit an.";
            }

            return errorMessage;
        }

        private string ValidateInterval()
        {
            string errorMessage = "";

            if (this.Interval == null || this.Interval <= 0)
            {
                errorMessage = "Das Serienmuster 'Intervall' ist ungültig, bitte geben Sie ein Intervall in Tag, Stunden, Minuten an.";
            }
            return errorMessage;
        }

        private string ValidateYearlyNth()
        {
            string errorMessage = "";

            if (!(this.DayInstance >= 1 && this.DayOfWeekMask != null && this.DayOfWeekMask != EmptyDayOfWeekMask && this.MonthOfYear > 0))
            {
                errorMessage = "Das Serienmuster 'Jährlich' ist ungültig.";
            }
            return errorMessage;
        }

        private string ValidateYearly()
        {
            string errorMessage = "";

            if (!(this.DayOfMonth >= 1 && this.DayOfMonth <= 31 && this.MonthOfYear > 0))
            {
                errorMessage = "Das Serienmuster 'Jährlich' ist ungültig";
            }
            return errorMessage;
        }

        private string ValidateMonthly()
        {
            string errorMessage = "";

            if (!(this.DayOfMonth >= 1 && this.DayOfMonth <= 31))
            {
                errorMessage = "Das Serienmuster 'Monatlich' ist ungültig, bitte geben Sie einen Tag zwischen 1 und 31 an.";
            }

            if (this.Interval == null || this.Interval < 0)
            {
                errorMessage = "Das Serienmuster 'Monatlich' ist ungültig, bitte geben Sie ein gültiges Intervall an.";
            }
            return errorMessage;
        }

        private string ValidateMonthlyNth()
        {
            string errorMessage = "";

            if (this.Interval == null || this.Interval < 0)
            {
                errorMessage = "Das Serienmuster 'Monatlich' ist ungültig, bitte geben Sie ein gültiges Intervall an.";
            }

            if (!(this.DayInstance >= 1 && this.DayOfWeekMask != null && this.DayOfWeekMask != EmptyDayOfWeekMask))
            {
                errorMessage = "Das Serienmuster 'Monatlich' ist ungültig";
            }
            return errorMessage;
        }

        private string ValidateWeekly()
        {
            string errorMessage = "";

            if (this.DayOfWeekMask == null || this.DayOfWeekMask == EmptyDayOfWeekMask)
            {
                errorMessage = "Das Serienmuster 'Wöchentlich' ist ungültig, bitte geben Sie mindestens einen Wochentag an.";
            }

            if (this.Interval == null || this.Interval < 1)
            {
                errorMessage = "Das Serienmuster 'Wöchentlich' ist ungültig, bitte geben Sie ein gültiges Intervall an.";
            }
            return errorMessage;
        }

        private string ValidateDaily()
        {
            string errorMessage = "";

            // Alle N Tage
            if ((this.DayOfWeekMask == null || this.DayOfWeekMask == EmptyDayOfWeekMask) && (this.Interval == null || this.Interval < 1))
            {
                errorMessage = "Das Serienmuster 'Täglich' ist ungültig, bitte geben Sie ein gültige Anzahl Tage an.";
            }
            else if ((this.DayOfWeekMask == null || this.DayOfWeekMask == WorkingDayOfWeekMask && this.Interval != 1))
            {
                errorMessage = "Das Serienmuster 'Täglich' ist ungültig";
            }

            //Ende vor Start
            if (this.From != null && this.To.HasValue)
            {
                if (this.FromTime > this.ToTime)
                {
                    errorMessage = "Das Serienmuster 'Täglich' ist ungültig (Ende liegt vor dem Anfang)";
                }
            }
            return errorMessage;
        }

        /// <summary>
        /// Gets or sets die ZeitkomponenteVon
        /// </summary>
        public virtual DateTime FromDate
        {
            get
            {
                return this.From.Date;
            }

            set
            {
                TimeSpan time = (TimeSpan)this.FromTime;
                this.From = value.Add(time);
            }
        }

        /// <summary>
        /// Gets or sets die Zeitkomponente Von
        /// </summary>
        public virtual TimeSpan? FromTime
        {
            get
            {
                return (this.From).TimeOfDay;
            }

            set
            {
                if (value != null)
                {
                    this.From = (this.From).Date.Add((TimeSpan)value);
                    decimal days = (decimal)((TimeSpan)this.ToTime - (TimeSpan)value).TotalDays;

                    if (days < 0)
                    {
                        days++;
                    }

                    this.DurationDays = days;
                }
            }
        }

        public virtual DateTime? ToDate
        {
            get
            {
                if (!this.To.HasValue)
                {
                    return null;
                }
                return this.To.Value.Date;
            }

            set
            {
                TimeSpan time = (TimeSpan)this.ToTime;
                if (value != null && value.HasValue)
                {
                    this.To = value.Value.Add(time);
                }
            }
        }

        /// <summary>
        /// Gets or sets die ZeitkomponenteBis
        /// </summary>
        public virtual TimeSpan? ToTime
        {
            get
            {
                if (this.DurationDays != null)
                {
                    this.toTime = this.FromTime.Value.Add(TimeSpan.FromDays((double)this.DurationDays));
                }
                else
                {
                    this.toTime = (this.From).TimeOfDay;
                }

                return this.toTime;
            }

            set
            {
                this.toTime = value;

                if (this.toTime != null)
                {
                    decimal days = (decimal)((TimeSpan)this.toTime - (TimeSpan)this.FromTime).TotalDays;

                    if (days < 0)
                    {
                        days++;
                    }

                    this.DurationDays = days;
                }
            }
        }

        public bool IsValidDayOfWeekMask(string dayOfWeekMask)
        {
            if (string.IsNullOrEmpty(dayOfWeekMask))
            {
                return false;
            }

            if (dayOfWeekMask.Length != FullDayOfWeekMask.Length)
            {
                return false;
            }

            for (int i = 0; i < FullDayOfWeekMask.Length; i++)
            {
                if (dayOfWeekMask.Substring(i, 1) != FullDayOfWeekMask.Substring(i, 1)
                 && dayOfWeekMask.Substring(i, 1) != EmptyDayOfWeekMask.Substring(i, 1))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsValidDayOfMonth(int? dayOfMonth)
        {
            if (dayOfMonth.HasValue && (dayOfMonth < 1 || dayOfMonth > 31))
            {
                return false;
            }
            return true;
        }

        public bool IsValidMonthOfYear(int? monthOfYear)
        {
            if (monthOfYear.HasValue && (monthOfYear < 1 || monthOfYear > 12))
            {
                return false;
            }
            return true;
        }
    }

    public class RecurrenceTimer
    {
        private Timer schedulTimer;
        private CancellationTokenSource cancellationTokenSource;
        private Task schedulerTask;

        public DateTime? PLastExecutionDate
        {
            get
            {
                return this.LastExecutionDate;
            }
            set
            {
                this.LastExecutionDate = value;
            }
        }

        public event EventHandler<EventArgs<object>> Tick;

        public RecurrencePattern RecurrencePattern
        {
            get
            {
                return this.recurrencePattern;
            }
        }

        /// <summary>
        /// Initializes the scheduler.
        /// </summary>
        public void InitializeScheduler()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.schedulerTask = Task.Factory.StartNew(this.Start, this.cancellationTokenSource.Token);
        }

        /// <summary>
        /// Starts the task scheduler.
        /// </summary>
        /// <param name="para">The para.</param>
        public void Start()
        {
            this.Stop();

            DateTime now = DateTime.Now;
            TimeSpan startTime = new TimeSpan(0, 0, 0);

            this.schedulTimer = new Timer(this.CheckScheduledTasks, null, startTime, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Stops the task scheduler.
        /// </summary>
        public void Stop()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
            }

            if (this.schedulTimer != null)
            {
                this.schedulTimer.Dispose();
                this.schedulTimer = null;
            }
        }

        private DateTime lastScheduelCheck;
        private DateTime lastHeartBeat = DateTime.Now;
        private long countHeartbeat = 0;

        private RecurrenceGenerator recurrenceGenerator = new RecurrenceGenerator();

        private RecurrencePattern recurrencePattern = new RecurrencePattern();

        /// <summary>
        /// The tolerance start shift
        /// </summary>
        private static readonly TimeSpan toleranceStartShift = new TimeSpan(0, 0, 0, 50, 0);

        public bool StartAfterMissedSchedul = false;
        public TimeSpan? ToleratedStartDelay = null;

        public DateTime? NextExecutionDate;

        public DateTime? LastExecutionDate;

        /// <summary>
        /// Checks the scheduled tasks.
        /// </summary>
        /// <param name="tmp">Nicht gebrauchter Parameter</param>
        private void CheckScheduledTasks(object tmp)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            TimeSpan nextTime = TimeSpan.Zero;

            this.lastScheduelCheck = DateTime.Now;

            try
            {
                var scheduleEntry = this.recurrencePattern;

                if (this.NextExecutionDate == null)
                {
                    this.NextExecutionDate = this.SetNextExecution(this.recurrencePattern);
                }
                DateTime? nextExecuteDate = this.NextExecutionDate;

                if (nextExecuteDate != null)
                {
                    DateTime? lastDate = this.LastExecutionDate;
                    DateTime now = DateTime.Now;

                    if (lastDate != null)
                    {
                        lastDate = lastDate.Value.SetSecond(0);
                        lastDate = lastDate.Value.SetMillisecond(0);
                    }

                    if (this.LastExecutionDate == null || nextExecuteDate.Value > lastDate)
                    {
                        bool start = false;

                        if (this.StartAfterMissedSchedul)
                        {
                            if (nextExecuteDate <= now)
                            {
                                start = true;
                            }
                        }
                        else
                        {
                            IList<DateTime> nextExecutes = this.recurrenceGenerator.GenerateRecurrences(scheduleEntry, null, this.LastExecutionDate ?? now, now);

                            foreach (DateTime date in nextExecutes)
                            {
                                nextExecuteDate = date;

                                TimeSpan delayTime = now.Subtract(this.lastScheduelCheck).Add(toleranceStartShift);

                                if (this.ToleratedStartDelay != null)
                                {
                                    delayTime = delayTime.Add(this.ToleratedStartDelay.Value);
                                }

                                if (nextExecuteDate <= now && nextExecuteDate >= now.Subtract(delayTime))
                                {
                                    start = true;
                                    break;
                                }
                            }
                        }

                        if (start)
                        {
                            if (this.Tick != null)
                            {
                                this.Tick.Invoke(this, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                sw.Stop();
                DateTime now = DateTime.Now;

                // zeitspanne bis zur nächsten vollen Minute
                nextTime = now.AddMinutes(1).SetSecond(0).SetMillisecond(0).Subtract(now);

                TimeSpan nextTimeCheck = new TimeSpan(nextTime.Days, nextTime.Hours, nextTime.Minutes, nextTime.Seconds, 0);
                TimeSpan lastTimeCheck = new TimeSpan(this.lastScheduelCheck.TimeOfDay.Days, this.lastScheduelCheck.TimeOfDay.Hours, this.lastScheduelCheck.TimeOfDay.Minutes, this.lastScheduelCheck.TimeOfDay.Seconds, 0);

                if (nextTime <= new TimeSpan(0, 0, 0, 0, 2))
                {
                    nextTime = new TimeSpan(0, 0, 0, 1, 0);
                }
                else if (Equals(nextTimeCheck, lastTimeCheck))
                {
                    nextTime = new TimeSpan(0, 0, 0, 1, 0);
                }

                if (this.cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (this.schedulTimer != null)
                    {
                        this.schedulTimer.Dispose();
                    }
                }
                else
                {
                    this.schedulTimer.Change(nextTime, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private DateTime? SetNextExecution(RecurrencePattern schedulEntry)
        {
            DateTime? nextDate = null;

            if (schedulEntry != null)
            {
                IList<DateTime> dates = this.recurrenceGenerator.GenerateRecurrences(schedulEntry, null, this.LastExecutionDate ?? DateTime.Now, DateTime.Now);

                if (dates != null && dates.Count > 0)
                {
                    if (this.LastExecutionDate != null)
                    {
                        dates = dates.Where(x => x > this.LastExecutionDate).ToList();
                    }

                    if (dates.Count > 0)
                    {
                        if (this.StartAfterMissedSchedul)
                        {
                            nextDate = dates.First();
                        }
                        else
                        {
                            nextDate = dates.Last();
                        }
                    }
                }

                if (nextDate != null && nextDate.Value.Second > 0)
                {
                    nextDate = nextDate.Value.AddMinutes(1).SetSecond(0);
                }
            }

            return nextDate;//this.CheckSchedul(newtask.SchedulEntry, false, null, DateTime.Now);
        }
    }
}