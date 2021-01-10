using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ChuckHill2.Extensions;
using ChuckHill2.Extensions.Reflection;

namespace ChuckHill2
{
    /// <summary>
    /// Automatic and Dynamic translation of all strings in an application into another language.
    /// </summary>
    public static class Internationalization
    {
        private static readonly CultureInfo OriginalCulture = CultureInfo.CurrentCulture; //used by Dispose()
        private static readonly Log LOG = new Log("General");

        #region enum Calendars
        public enum Calendars //Constants copied from class System.Globalization.Calendar
        {
            Gregorian = 1,
            Gregorian_US = 2,
            JapaneseEmperorEra = 3,
            TaiwanEra = 4,
            KoreanTangunEra = 5,
            Hijri_ArabicLunar = 6,
            Thai = 7,
            Hebrew_Lunar = 8,
            Gregorian_MiddleEastFrench = 9,
            Gregorian_Arabic = 10,
            Gregorian_EnglishLabels = 11,
            Gregorian_FrenchLabels = 12,
            Julian = 13,
            JapaneseLunisolar = 14,
            ChineseLunisolar = 15,
            Saka = 16,                 // reserved to match Office but not implemented
            Chinese_LunarETO = 17,     // reserved to match Office but not implemented
            Korean_LunarETO = 18,      // reserved to match Office but not implemented
            Rokuyou_LunarETO = 19,     // reserved to match Office but not implemented
            Korean_Lunisolar = 20,
            Taiwanese_Lunisolar = 21,
            Persian = 22,
            UmAlQura = 23
        }
        #endregion

        /// <summary>
        /// Set this thread and entire AppDomain to the specified culture.
        /// Must be called at the beginning of every AppDomain startup.
        /// Use Internationalization.Dispose() to reset back to computer system culture.
        /// </summary>
        /// <param name="cultureName">
        /// ITEF culture name or ("","iv","iv-IV") for the invariant culture.
        /// If null, retrieves the culturename from the environment variable "[productname]Culture".
        /// If still null, this function does nothing and the app defaults to the current culture of the computer.
        /// </param>
        public static void SetDomainCulture(string cultureName=null)
        {
            if (CultureInfo.DefaultThreadCurrentCulture == null)
            {
                //CultureInfo.DefaultThreadCurrentCulture is NULL if never used! We need it for culture reset.
                CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            }

            if (cultureName == null) cultureName = Environment.GetEnvironmentVariable(Resolver.ProductName + "Culture");
            if (cultureName == null) return; //nothing to do! No overrides. Use System culture.

            SetThreadCulture(cultureName); 
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
        }

        /// <summary>
        /// Set the current thread to an alternate culture via ITEF culture name.
        /// </summary>
        /// <param name="cultureName">
        /// ITEF culture name or ("","iv","iv-IV") for the invariant culture.
        /// </param>
        public static void SetThreadCulture(string cultureName = null)
        {
            try
            {
                if (cultureName == null) //RESET
                {
                    if (Thread.CurrentThread.CurrentCulture.ToString() == CultureInfo.DefaultThreadCurrentCulture.ToString()) return;
                    LOG.Information("Resetting thread culture {0} back to {1}.", Thread.CurrentThread.CurrentCulture, CultureInfo.DefaultThreadCurrentCulture);
                    CultureInfo.CurrentCulture.ClearCachedData();
                    Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture;
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture;
                    return;
                }

                if (cultureName.EqualsI("iv") || cultureName.EqualsI("iv-IV")) cultureName = string.Empty;
                string currentName = Thread.CurrentThread.CurrentCulture.ToString();

                if (currentName.EqualsI(cultureName)) return; //nothing to do!

                var ci = CultureInfo.GetCultureInfo(cultureName);
                if (ci.ToString() == string.Empty) //Special: must be invariant culture. Fixup.
                {
                    //Give empty Invariant name a nice name for logging purposes. Internally, CultureInfo.m_name" 
                    //is used exclusively by CultureInfo.ToString() but NOT always by CultureInfo.Name property!
                    //ci.SetReflectedValue("m_name", "iv-IV"); 
                    //Oops! The Xml Reader DOES use new Culture(CultureInfo.ToString()) and pseudo-culture "iv-IV" will not be found!

                    //Invariant same as en-US but with a 24-hr clock and a '¤' currency symbol.
                    ci.NumberFormat.SetReflectedValue("currencySymbol", "$");
                    ci.NumberFormat.SetReflectedValue("ansiCurrencySymbol", "$");
                }

                //We can't handle non-gregorian calendars!
                if (!(ci.Calendar is GregorianCalendar))
                {
                    Calendar newcal = ci.OptionalCalendars.FirstOrDefault(m => m is GregorianCalendar && ((GregorianCalendar)m).CalendarType == GregorianCalendarTypes.Localized);
                    if (newcal == null) newcal = ci.OptionalCalendars.FirstOrDefault(m => m is GregorianCalendar);
                    if (newcal != null)
                    {
                        ci.SetReflectedValue("calendar", newcal);
                        ci.DateTimeFormat.SetReflectedValue("calendar", newcal);
                    }
                }

                LOG.Information("Replacing culture {0} with {1}.", (currentName == "" ? "iv-IV" : currentName), (ci.ToString() == "" ? "iv-IV" : ci.ToString()));
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                CultureInfo.CurrentCulture.ClearCachedData();
            }
            catch (Exception ex)
            {
                LOG.Error(ex, "SetThreadCulture(\"{0}\")", cultureName == "" ? "iv-IV" : cultureName);
            }
        }

        /// <summary>
        /// Safely determine if the ITEF culture exists without exceptions.
        /// </summary>
        /// <param name="cultureName">ITEF culture name</param>
        /// <returns>true if exists</returns>
        public static bool CultureExists(string cultureName)
        {
            return (CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(m => m.ToString().EqualsI(cultureName)) != null);
        }

        /// <summary>
        /// Delete a custom culture. Requires administrative privileges.
        /// </summary>
        /// <param name="cultureName"></param>
        public static void DeleteCulture(string cultureName)
        {
            //Only allow custom cultures to be deleted.
            if (CultureInfo.GetCultures(CultureTypes.UserCustomCulture).FirstOrDefault(m => m.ToString().EqualsI(cultureName)) != null)
            {
                CultureAndRegionInfoBuilder.Unregister(cultureName);
                //The underlying *.nlp files will never be able to be deleted because they are always 
                //in-use by some other application, in particular Windows Explorer! Unregister removes 
                //it from the registry and rename the .nlp file to .tmp0, but never deletes the .tmp0 
                //file. We add a little bit extra to delete the dead .tmp0 file upon reboot.
                foreach (var f in Directory.EnumerateFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Globalization"), "*.tmp?", SearchOption.TopDirectoryOnly))
                {
                    MoveFileEx(f, null, MoveFileFlags.DELAY_UNTIL_REBOOT);
                }
            }
        }

        /// <summary>
        /// Nice friendly calendar name for display purposes only.
        /// </summary>
        /// <param name="cal"></param>
        /// <returns></returns>
        public static string FriendlyName(this Calendar cal) 
        { 
            switch(cal.ID())
            {
                case Calendars.Gregorian:                  return "Gregorian Generic";
                case Calendars.Gregorian_US:               return "Gregorian US English";
                case Calendars.Gregorian_MiddleEastFrench: return "Gregorian Middle East French";
                case Calendars.Gregorian_Arabic:           return "Gregorian Arabic";
                case Calendars.Gregorian_EnglishLabels:    return "Gregorian Transliterated English";
                case Calendars.Gregorian_FrenchLabels:     return "Gregorian Transliterated French";
                case Calendars.Julian:                     return "Julian";
                case Calendars.JapaneseEmperorEra:         return "Japanese Emperor Era";
                case Calendars.JapaneseLunisolar:          return "Japanese";
                case Calendars.TaiwanEra:                  return "Taiwanese Era";
                case Calendars.Taiwanese_Lunisolar:        return "Taiwanese";
                case Calendars.ChineseLunisolar:           return "Chinese";
                case Calendars.KoreanTangunEra:            return "Korean Tangun Era";
                case Calendars.Korean_Lunisolar:           return "Korean";
                case Calendars.Hijri_ArabicLunar:          return "Hijri";
                case Calendars.UmAlQura:                   return "Umm al-Qura";
                case Calendars.Persian:                    return "Persian";
                case Calendars.Thai:                       return "Thai Buddhist";
                case Calendars.Hebrew_Lunar:               return "Hebrew";

                case Calendars.Saka:                       return "Hindu Saka";
                case Calendars.Chinese_LunarETO:           return "Chinese Lunar";
                case Calendars.Korean_LunarETO:            return "Korean Lunar";
                case Calendars.Rokuyou_LunarETO:           return "Japanese Roku-You Lunar";
                default:                                   return cal.ID().ToString();
            }
        }
        /// <summary>
        /// Valid RDLC parameter name for the calendars. Not necessarily friendly.
        /// </summary>
        /// <param name="cal"></param>
        /// <returns></returns>
        public static string RdlcName(this Calendar cal) 
        { 
            switch(cal.ID())
            {
                case Calendars.Gregorian:                  return "Gregorian";
                case Calendars.Gregorian_US:               return "Gregorian US English";
                case Calendars.Gregorian_MiddleEastFrench: return "Gregorian Middle East French";
                case Calendars.Gregorian_Arabic:           return "Gregorian Arabic";
                case Calendars.Gregorian_EnglishLabels:    return "Gregorian Transliterated English";
                case Calendars.Gregorian_FrenchLabels:     return "Gregorian Transliterated French";
                case Calendars.Julian:                     return "Julian";
                case Calendars.JapaneseEmperorEra:         return "Japanese";
                case Calendars.JapaneseLunisolar:          return "Japanese";
                case Calendars.TaiwanEra:                  return "Taiwan";
                case Calendars.Taiwanese_Lunisolar:        return "Taiwan";
                case Calendars.ChineseLunisolar:           return "Taiwan";
                case Calendars.KoreanTangunEra:            return "Korea";
                case Calendars.Korean_Lunisolar:           return "Korean";
                case Calendars.Hijri_ArabicLunar:          return "Hijri";
                case Calendars.Persian:                    return "Persian";
                case Calendars.UmAlQura:                   return "UmAlQura";
                case Calendars.Thai:                       return "Thai Buddhist";
                case Calendars.Hebrew_Lunar:               return "Hebrew";

                case Calendars.Saka:                       return "Indian";
                case Calendars.Chinese_LunarETO:           return "Chinese";
                case Calendars.Korean_LunarETO:            return "Korea";
                case Calendars.Rokuyou_LunarETO:           return "Japanese";
                default:                                   return cal.ID().ToString();
            }
        }

        /// <summary>
        /// Calendar extension for getting calendar identifier 
        /// Internationalization.Calendars enum value for specified calendar.
        /// </summary>
        /// <param name="cal"></param>
        /// <returns></returns>
        public static Calendars ID(this Calendar cal)
        {
            if (cal==null) return (Calendars)0;
            return (Calendars)cal.GetReflectedValue("ID");
        }

        #region Win32 MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags)
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        [Flags] enum MoveFileFlags
        {
            REPLACE_EXISTING = 0x00000001,
            COPY_ALLOWED = 0x00000002,
            DELAY_UNTIL_REBOOT = 0x00000004,
            WRITE_THROUGH = 0x00000008,
            CREATE_HARDLINK = 0x00000010,
            FAIL_IF_NOT_TRACKABLE = 0x00000020
        }
        #endregion

        /// <summary>
        /// There is CultureInfo.Equals() always returns true, so we have
        /// to implement our own that compares at every public property.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool CulturesEqual(CultureInfo a, CultureInfo b)
        {
            //Equality is not implemented properly because the pair will ALWAYS be equal!
            if (!a.ToString().EqualsI(b.ToString())) return false;

            if ((int)a.Calendar.ID() != (int)b.Calendar.ID()) return false;
            if (a.Calendar.TwoDigitYearMax != b.Calendar.TwoDigitYearMax) return false;

            if (a.DateTimeFormat.FirstDayOfWeek != b.DateTimeFormat.FirstDayOfWeek) return false;
            if (a.DateTimeFormat.ShortDatePattern != b.DateTimeFormat.ShortDatePattern) return false;
            if (a.DateTimeFormat.ShortTimePattern != b.DateTimeFormat.ShortTimePattern) return false;
            if (a.DateTimeFormat.LongDatePattern != b.DateTimeFormat.LongDatePattern) return false;
            if (a.DateTimeFormat.LongTimePattern != b.DateTimeFormat.LongTimePattern) return false;
            if (a.DateTimeFormat.PMDesignator != b.DateTimeFormat.PMDesignator) return false;
            if (a.DateTimeFormat.AMDesignator != b.DateTimeFormat.AMDesignator) return false;
            if (a.NumberFormat.CurrencySymbol != b.NumberFormat.CurrencySymbol) return false;

            if (a.NumberFormat.CurrencyPositivePattern != b.NumberFormat.CurrencyPositivePattern) return false;
            if (a.NumberFormat.CurrencyNegativePattern != b.NumberFormat.CurrencyNegativePattern) return false;
            if (a.NumberFormat.CurrencyDecimalSeparator != b.NumberFormat.CurrencyDecimalSeparator) return false;
            if (a.NumberFormat.CurrencyGroupSeparator != b.NumberFormat.CurrencyGroupSeparator) return false;
            if (!a.NumberFormat.CurrencyGroupSizes.SequenceEqual(b.NumberFormat.CurrencyGroupSizes)) return false;
            if (a.NumberFormat.NaNSymbol != b.NumberFormat.NaNSymbol) return false;
            if (!a.NumberFormat.NativeDigits.SequenceEqual(b.NumberFormat.NativeDigits)) return false;
            if (a.NumberFormat.NegativeInfinitySymbol != b.NumberFormat.NegativeInfinitySymbol) return false;
            if (a.NumberFormat.NegativeSign != b.NumberFormat.NegativeSign) return false;
            if (a.NumberFormat.NumberDecimalDigits != b.NumberFormat.NumberDecimalDigits) return false;
            if (a.NumberFormat.NumberDecimalSeparator != b.NumberFormat.NumberDecimalSeparator) return false;
            if (a.NumberFormat.NumberGroupSeparator != b.NumberFormat.NumberGroupSeparator) return false;
            if (!a.NumberFormat.NumberGroupSizes.SequenceEqual(b.NumberFormat.NumberGroupSizes)) return false;
            if (a.NumberFormat.NumberNegativePattern != b.NumberFormat.NumberNegativePattern) return false;
            if (a.NumberFormat.PercentDecimalDigits != b.NumberFormat.PercentDecimalDigits) return false;
            if (a.NumberFormat.PercentDecimalSeparator != b.NumberFormat.PercentDecimalSeparator) return false;
            if (a.NumberFormat.PercentGroupSeparator != b.NumberFormat.PercentGroupSeparator) return false;
            if (!a.NumberFormat.PercentGroupSizes.SequenceEqual(b.NumberFormat.PercentGroupSizes)) return false;
            if (a.NumberFormat.PercentNegativePattern != b.NumberFormat.PercentNegativePattern) return false;
            if (a.NumberFormat.PercentPositivePattern != b.NumberFormat.PercentPositivePattern) return false;
            if (a.NumberFormat.PercentSymbol != b.NumberFormat.PercentSymbol) return false;
            if (a.NumberFormat.PerMilleSymbol != b.NumberFormat.PerMilleSymbol) return false;
            if (a.NumberFormat.PositiveInfinitySymbol != b.NumberFormat.PositiveInfinitySymbol) return false;
            if (a.NumberFormat.PositiveSign != b.NumberFormat.PositiveSign) return false;
            if (a.NumberFormat.DigitSubstitution != b.NumberFormat.DigitSubstitution) return false;

            return true;
        }

        /// <summary>
        /// Cleanup, flush, save state properties.
        /// This should be the last thing in the AppDomain's Program.Main().
        /// </summary>
        public static void Dispose()
        {
            CultureInfo.CurrentCulture.ClearCachedData();
            Thread.CurrentThread.CurrentCulture = OriginalCulture;
            Thread.CurrentThread.CurrentUICulture = OriginalCulture;
            CultureInfo.DefaultThreadCurrentCulture = OriginalCulture;
            CultureInfo.DefaultThreadCurrentUICulture = OriginalCulture;
        }

        #region Language Translation (DISABLED)
        #if LANGUAGETRANSLATION
        #region Private Fields
        private static TranslationDictionary TranslatedCache = null;
        private const string DefaultLanguage = "en"; //This is the language that all strings in this application are in (aka. English).
        private static string CurrentLanguage = DefaultLanguage; //The current language to translate into.
        #endregion

        /// <summary>
        /// Initialize automatic language translation. If the current or specified culture is "en-US" (English), 
        /// this does nothing. This should be called at the beginning of Program.Main() before any UI is displayed. 
        /// The culture to use is determined by the following:
        ///  (1) the specified argument 'cultureCode'.
        ///  (2) if null, the environment variable '[product]Culture'
        ///  (3) if null, the Current culture for this computer.
        ///      (at this point, the culture is set for the application)
        ///  (4) the environment variable '[product]LanguageTranslate' is true
        ///  (4) if languageCode is supported by GoogleTranslate.
        ///  (5) if languageCode is NOT 'en' (English)
        ///  (5) otherwise no translation.
        ///  Note: This API must be called at the beginning of every AppDomain.
        /// </summary>
        /// <param name="cultureCode">optional 2(maybe 3)-letter language code (e.g. "en") optionally followed by '-' and a locale code (e.g. "en-US" or "en-UK")</param>
        public static void Initialize(string cultureCode = null)
        {
            try
            {
                bool manuallySet = true;
                if (cultureCode.IsNullOrEmpty()) cultureCode = Environment.GetEnvironmentVariable(Resolver.ProductName + "Culture");
                if (cultureCode.IsNullOrEmpty()) { cultureCode = CultureInfo.CurrentCulture.ToString(); manuallySet = false; }
                bool translate = Environment.GetEnvironmentVariable(Resolver.ProductName + "LanguageTranslate").Cast<bool>();
                string currentCultureName = CultureInfo.CurrentCulture.ToString();
                CultureInfo cultureInfo = null;

                //Handle when user changes settings in the 'Region and Language' dialog. 
                //ONLY if the user did not explicitly set the culture for just this app.
                if (!manuallySet)
                {
                    SystemEvents.UserPreferenceChanged -= CultureChanged; //We should only set this once!
                    SystemEvents.UserPreferenceChanged += CultureChanged;
                }

                LOG.LogInformation("Setting Culture to {0}", cultureCode);

                if (CultureExists(cultureCode))
                    cultureInfo = new CultureInfo(cultureCode, true);
                else
                {
                    var parts = cultureCode.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    string language = parts[0].ToLower();
                    string region = parts.Length > 1 ? parts[1].ToUpper() : string.Empty;
                    if (!region.IsNullOrEmpty())
                    {
                        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(m => m.ToString().StartsWith(language)).ToList().ToString(',', m => " \"" + m.ToString() + "\"");
                        LOG.LogWarning("Invalid Culture: {0}. Trying again without region/country. Valid culture codes for {1} are:{2}", cultureCode, language, cultures);
                        if (CultureExists(language))
                            cultureInfo = new CultureInfo(language, true);
                        else LOG.LogWarning("Invalid Culture: {0}. Culture not set.", language);
                    }
                    else LOG.LogWarning("Invalid Culture: {0}. Culture not set.", cultureCode);
                }

                if (cultureInfo != null)
                {
                    cultureInfo = OverrideCulture(cultureInfo);
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo; //controls default number and date formatting and the like.
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo; //user interface language
                }

        #region Translate the embedded strings into current language
                //Translate the embedded strings in the application FOR THIS APPDOMAIN ONLY.
                if (translate)
                {
                    Internationalization.CurrentLanguage = (cultureInfo == null ? CultureInfo.CurrentCulture : cultureInfo).TwoLetterISOLanguageName;
                    if (CurrentLanguage == DefaultLanguage) return;
                    //See Supported Language Codes: http://msdn.microsoft.com/en-us/library/ms533052%28v=vs.85%29.aspx
                    //Google Translate contains a subset of these. See GoogleLanguageCodes dictionary.
                    if (!GoogleLanguageCodes.ContainsKey(CurrentLanguage))  //If Google doesn't support it, we don't either...
                    {
                        LOG.LogWarning("Language Not Supported: {0}. Translation Disabled.", cultureCode);
                        return;
                    }
                    LoadCache();
                    InitDetection(true);
                }
        #endregion
            }
            catch (Exception ex)
            {
                LOG.LogWarning(ex, "Localization Initialization Error for \"{0}\"", cultureCode);
            }
        }

        private static void CultureChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.Locale) return;
            LOG.LogInformation("Resetting CultureInfo.");
            Dispose();
            Initialize();
        }

        /// <summary>
        /// Replace CultureInfo Calendars that will cause exceptions in our code.
        /// </summary>
        /// <param name="ci"></param>
        private static CultureInfo OverrideCulture(CultureInfo ci)
        {
            if (ci.Calendar.MinSupportedDateTime > new DateTime(1, 1, 1) || ci.Calendar.MaxSupportedDateTime < new DateTime(2100, 1, 1))
            {
                //The following cultures are problemmatic. When the minimum supported date is greater than 01/01/0001 exceptions
                //are thrown throughout the code. Many are deep within 3rd-party code and cannot be fixed. In our code we fix 
                //this by replacing the Calendar on-the-fly, so we do not have these problems. The following are currently not 
                //supported for Microsoft Report Viewer Service...Ever. This can be remedied by by NOT passing any datetime objects to 
                //the RDLC's but pre-format the datetime values into strings and passing the string result instead.

                //-------------------------------------------------------------------------------------------------------------------------------
                //Code   Name                  Calendar          Format               1stDayOfWeek MaxDate                 MinDate
                //-------------------------------------------------------------------------------------------------------------------------------
                //ar     Arabic                UmAlQura          dd/MM/yy hh:mm:ss tt Saturday     2077-11-16T23:59:59.999 1900-04-30T00:00:00
                //ar-SA  Arabic (Saudi Arabia) UmAlQura          dd/MM/yy hh:mm:ss tt Saturday     2077-11-16T23:59:59.999 1900-04-30T00:00:00
                //prs    Dari                  Hijri_ArabicLunar dd/MM/yy h:mm:ss tt  Friday       9999-12-31T23:59:59.999 0622-07-18T00:00:00
                //ps     Pashto                Hijri_ArabicLunar dd/MM/yy h:mm:ss tt  Saturday     9999-12-31T23:59:59.999 0622-07-18T00:00:00
                //dv     Divehi                Hijri_ArabicLunar dd/MM/yy HH:mm:ss    Sunday       9999-12-31T23:59:59.999 0622-07-18T00:00:00
                //ps-AF  Pashto (Afghanistan)  Hijri_ArabicLunar dd/MM/yy h:mm:ss tt  Saturday     9999-12-31T23:59:59.999 0622-07-18T00:00:00
                //dv-MV  Divehi (Maldives)     Hijri_ArabicLunar dd/MM/yy HH:mm:ss    Sunday       9999-12-31T23:59:59.999 0622-07-18T00:00:00
                //prs-AF Dari (Afghanistan)    Hijri_ArabicLunar dd/MM/yy h:mm:ss tt  Friday       9999-12-31T23:59:59.999 0622-07-18T00:00:00
                //-------------------------------------------------------------------------------------------------------------------------------

                //All the problemmatic calendars are arabic, so we will first look for an arabic variant of the Gregorian calendar.
                Calendars prevId = ci.Calendar.ID(); //This is not public! Go figure.
                Calendar cal = ci.OptionalCalendars.FirstOrDefault(m => m is GregorianCalendar && ((GregorianCalendar)m).CalendarType == GregorianCalendarTypes.Arabic);
                if (cal == null) cal = ci.OptionalCalendars.FirstOrDefault(m => m is GregorianCalendar && ((GregorianCalendar)m).CalendarType == GregorianCalendarTypes.Localized);
                if (cal == null) cal = ci.OptionalCalendars.FirstOrDefault(m => m is GregorianCalendar);
                if (cal != null)
                {
                    Calendars Id = cal.ID(); //This is not public! Go figure.
                    //ReadOnly test is only needed for cultures read directly from the internal cache eg. CultureInfo.GetCultureInfo("en-GB") instead of new CultureInfo("en-GB");
                    bool isReadOnly = (bool)ci.DateTimeFormat.GetReflectedValue("m_isReadOnly");
                    ci.DateTimeFormat.SetReflectedValue("m_isReadOnly", false);
                    ci.DateTimeFormat.Calendar = cal;
                    ci.DateTimeFormat.SetReflectedValue("m_isReadOnly", isReadOnly);
                    ci.SetReflectedValue("calendar", cal);
                    ci.DateTimeFormat.SetReflectedValue("m_isDefaultCalendar", true);
                    LOG.LogInformation("Resetting Culture {0} calendar from {1} to {2}", ci.ToString(), prevId, Id);
                }
            }

            ////Create our own custom CultureInfo only if it is different from the original baseline CultureInfo.
            ////http://codinginthetrenches.com/2014/04/03/net-custom-cultures-and-sqlserver-reporting-services/
            ////Arrgh!! We can't create our own custom culture and use it because ReportViewer, and ASP.NET in 
            ////general (DataSets, etc), use LCID's (0x0409) instead of the newer IETF tags (e.g. "en-US"). 
            ////LCID's do not support custom Cultures/Locales.
            //if ((new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
            //{
            //    //Do this ONLY if we have administrative priveleges
            //    CultureInfo unmodifiedCI = new CultureInfo(ci.ToString(), false);
            //    if (!CulturesEqual(unmodifiedCI, ci))
            //    {
            //        var builder = new CultureAndRegionInfoBuilder(ci.ToString(), CultureAndRegionModifiers.Replacement);
            //        //Add changes to builder
            //        builder.LoadDataFromCultureInfo(ci);
            //        builder.LoadDataFromRegionInfo((RegionInfo)ci.GetReflectedValue("Region"));
            //        //Commit changes
            //        try { Internationalization.DeleteCulture(ci.ToString()); }
            //        catch (Exception ex) { LOG.LogError(ex, "DeleteCulture(\"{0}\")", ci.ToString()); }
            //        try { builder.Register(); }
            //        catch (Exception ex) { LOG.LogError(ex, "RegisterCulture(\"{0}\")", ci.ToString()); }
            //    }
            //}

            return ci;
        }

        /// <summary>
        /// Cleanup and save the language cache for future instances of this application.
        /// This should be the last thing in Program.Main().
        /// </summary>
        public static void Dispose()
        {
            InitDetection(false);
            SaveCache();
            CultureInfo.CurrentCulture.ClearCachedData();
        }

        /// <summary>
        /// Translate US-English text string into the language of the current/specified culture.
        /// </summary>
        /// <param name="source">US-English text string</param>
        /// <returns>translated string. If there is problem in translation, the source string is returned. See the event log for details.</returns>
        public static string Translate(string source)
        {
            return Translate(source, CurrentLanguage);
        }

        #region Control Detection
        //System.Windows.Forms.Application.AddMessageFilter(FormCreateDetector) this doesn't recieve all the window messages!
        //System.Windows.Forms.Application.RemoveMessageFilter(FormCreateDetector);
        #region Win32
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_CALLWNDPROCRET = 12; //gets window management msgs AFTER window processed them
        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public Win32.WM message;
            public IntPtr hwnd;
        }

        private const int WH_CALLWNDPROC = 4;     //gets window management msgs BEFORE window processed them
        [StructLayout(LayoutKind.Sequential)]
        private struct CWPSTRUCT
        {
            public IntPtr lParam;
            public IntPtr wParam;
            public Win32.WM message;
            public IntPtr hwnd;
        }

        private const int WH_GETMESSAGE = 3;      //gets keyboard, mouse, and timer msgs only
        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public Win32.WM message;
            public IntPtr wParam;
            public IntPtr lParam;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnhookWindowsHookEx(IntPtr idHook);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")] 
        private static extern int GetCurrentThreadId();
        #endregion
        private static IntPtr _hActiveHook = IntPtr.Zero;
        private static readonly HookProc _activeHookProc = new HookProc(ActiveHookProc); //this MUST be static so it won't get garbage collected!
        private static IntPtr ActiveHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hActiveHook, nCode, wParam, lParam);
            CWPRETSTRUCT m = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            if (m.message == Win32.WM.WM_CREATE)
            {
                Control ctrl = Control.FromHandle(m.hwnd);
                if (ctrl is Form)
                {
                    Form f = ctrl as Form;
                    //Translate late as possible because child controls are usually not fully initialized until Form.OnLoad() is complete.
                    //We *could* translate what we have immediately upon WM_CREATE and just subscribe to change events, but this is not 
                    //trivial when the child controls are Infragistics due to the requirement of NOT including Infragistics references 
                    //into this DLL. If we could, it would be a lot more efficient than this late recursion.
                    f.Load += delegate(object sender, EventArgs e)
                    {
                        Form form = sender as Form;
                        //DBG.RawWrite(string.Format("0x{0:X8} [OnLoad()]\tType={1}\n", form.Handle.ToInt32(), form.GetType().FullName));
                        TranslateControl((Form)sender);
                    };
                }
            }

            //WndProcDEBUG(m.hwnd, (int)m.message);

            if (m.message == Win32.WM.WM_DESTROY)
            {
                TranslatedControls.Remove(m.hwnd.ToInt32()); //Cleanup: remove unused window handles.
            }

            return CallNextHookEx(_hActiveHook, nCode, wParam, lParam);
        }
        private static void WndProcDEBUG(IntPtr hWnd, int msg) //Debugging tool
        {
            try
            {
                if (msg == (int)Win32.WM.WM_SETCURSOR ||
                    msg == (int)Win32.WM.WM_MOUSEMOVE ||
                    msg == (int)Win32.WM.WM_NCHITTEST) return;

                Control ctrl = Control.FromHandle(hWnd);
                string type = (ctrl == null ? "UNKNOWN" : ctrl.GetType().FullName);
                DBG.RawWrite(string.Format("0x{0:X8} {1}\tType={2}\n", hWnd.ToInt32(), Win32.TranslateWMMessage(hWnd, msg), type));
            }
            catch (Exception ex)
            {
                DBG.RawWrite("WndProcDEBUG Error: " + ex.Message + "\n");
            }
        }
        private static void InitDetection(bool init)
        {
            if (init)
            {
                if (_hActiveHook != IntPtr.Zero) return;
                //Forms uses one thread almost exclusively. WPF uses many threads!
                //var th = Process.GetCurrentProcess().Threads[0]; //might be able to use this, but there are no add/remove events, so count polling is the only way. arrgh!
                //Hook procs can only be thread-specific or System-wide. Not process-specific. Go figure....
                //_hActiveHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _activeHookProc, IntPtr.Zero, 0);  //current process specific? NO. =="Cannot set nonlocal hook without a module handle."
                _hActiveHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _activeHookProc, IntPtr.Zero, GetCurrentThreadId()); //current process, current thread
                if (_hActiveHook == IntPtr.Zero) LOG.LogError(new Win32Exception("SetWindowsHookEx","Internationalization.SetWindowsHookEx(WH_CALLWNDPROCRET) failed."),"");
            }
            else
            {
                if (_hActiveHook == IntPtr.Zero) return;
                UnhookWindowsHookEx(_hActiveHook);
                _hActiveHook = IntPtr.Zero;
            }
        }
        #endregion

        #region Language Cache Persistence
        private static int CacheCount = 0; //This is how we detect if the cache needs to be saved
        private static void LoadCache()
        {
            string path = GetReadableLanguageFile(); //Just checks the directory exists and is writeable
            if (path == null)
            {
                TranslatedCache = new TranslationDictionary(CurrentLanguage);
                return;
            }

            //good explanation: http://www.codeproject.com/Articles/16164/Managed-Application-Shutdown
            //This does not seem to work! Just call Internationalization.SaveCache() as the the last line in Program.Main().
            AppDomain dom = AppDomain.CurrentDomain;
            if (dom.IsDefaultAppDomain()) dom.UnhandledException += AppDomainUnhandledException;
            else dom.DomainUnload += AppDomainUnload;
            dom.ProcessExit += AppDomainUnload;

            if (!File.Exists(path)) return;
            TranslatedCache = TranslationDictionary.Deserialize(path);
            CacheCount = TranslatedCache.Count;
        }
        private static void SaveCache()
        {
            //This does not seem to work! Just call Internationalization.SaveCache() as the the last line in Program.Main().
            AppDomain dom = AppDomain.CurrentDomain;
            if (dom.IsDefaultAppDomain()) dom.UnhandledException -= AppDomainUnhandledException;
            else dom.DomainUnload -= AppDomainUnload;
            dom.ProcessExit -= AppDomainUnload;

            if (TranslatedCache == null || TranslatedCache.Count == 0) return;
            if (CacheCount == TranslatedCache.Count) return; //no change
            string path = GetWritableLanguageFile(); //Just checks the directory exists and is writeable
            if (path == null) return;
            TranslatedCache.Serialize(path);

            CacheCount = TranslatedCache.Count;
        }

        private static void AppDomainUnload(object sender, EventArgs ev) { Dispose(); }
        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) { Dispose(); }
        private static string GetWritableLanguageFile()
        {
            string filename = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".LanguageTable." + CurrentLanguage + ".xml").Replace(".vshost", "");
            filename = Path.GetFileName(filename);

            string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            if (HasFolderWriteAccess(folder)) return Path.Combine(folder,filename);

            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),Resolver.ProductName);
            if (HasFolderWriteAccess(folder)) return Path.Combine(folder, filename);

            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Resolver.ProductName);
            if (HasFolderWriteAccess(folder)) return Path.Combine(folder, filename);
            
            return null;
        }
        private static string GetReadableLanguageFile()
        {
            string filename = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".LanguageTable." + CurrentLanguage + ".xml").Replace(".vshost", "");
            filename = Path.GetFileName(filename);

            string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string fullname = Path.Combine(folder, filename);
            if (File.Exists(fullname)) return fullname;

            folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            fullname = Path.Combine(folder, Resolver.ProductName, filename);
            if (File.Exists(fullname)) return fullname;

            folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); //this should always be successful!
            fullname = Path.Combine(folder, Resolver.ProductName, filename);
            if (File.Exists(fullname)) return fullname;

            return null;
        }
        private static bool HasFolderWriteAccess(string folder)
        {
            string path = null;
            try
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                path = Path.Combine(folder, string.Format("AccessTest{0:08X}.txt", Environment.TickCount));
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, 1, FileOptions.DeleteOnClose))
                {
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private class TranslationDictionary : IEnumerable<KeyValuePair<string, string>>, IEnumerable
        {
            private string _LanguageCode = "en";
            private Dictionary<string, string> d1;
            private Dictionary<string, string> d2;

            public string LanguageCode { get { return _LanguageCode; } protected set { _LanguageCode = value; } }

            IEqualityComparer<string> Comparer { get { return d2.Comparer; } }

            public TranslationDictionary()
            {
                IEqualityComparer<string> cIn = StringComparer.Create(CultureInfo.CreateSpecificCulture("en-US"), true);
                d1 = new Dictionary<string, string>(cIn);
                d2 = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            }
            public TranslationDictionary(string languageCode)
            {
                IEqualityComparer<string> cIn = StringComparer.Create(CultureInfo.CreateSpecificCulture("en-US"),true);
                IEqualityComparer<string> cOut;

                if (IsValidCulture(languageCode))
                {
                    try { cOut = StringComparer.Create(CultureInfo.CreateSpecificCulture(languageCode), true); _LanguageCode = languageCode; }
                    catch { cOut = StringComparer.InvariantCultureIgnoreCase; }
                }
                else cOut = StringComparer.InvariantCultureIgnoreCase;

                d1 = new Dictionary<string, string>(cIn);
                d2 = new Dictionary<string, string>(cOut);
            }
            public TranslationDictionary(IEqualityComparer<string> langComparer)
            {
                IEqualityComparer<string> cIn = StringComparer.Create(CultureInfo.CreateSpecificCulture("en-US"), true);
                Object compareInfo = langComparer.GetReflectedValue("_compareInfo");
                if (compareInfo!=null) _LanguageCode = compareInfo.GetReflectedValue("Name") as string;
                d1 = new Dictionary<string, string>(cIn);
                d2 = new Dictionary<string, string>(langComparer);
            }

            private bool IsValidCulture(string languageCode)
            {
                return (CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).FirstOrDefault(m => m.TwoLetterISOLanguageName.EqualsI(languageCode))!=null);
            }

            public int Count { get { return d1.Count; } }
            public Dictionary<string, string>.KeyCollection Keys { get { return d1.Keys; } }
            public Dictionary<string, string>.ValueCollection Values { get { return d1.Values; } }

            //unknown key will not throw exception. will just return null.
            public string this[string key] 
            {
                get
                {
                    string value = null;
                    d1.TryGetValue(key, out value);
                    return value;
                }
                set
                {
                    d1[key] = value;
                    d2[value] = key;
                }
            }

            //will not throw exception if key already exists.
            public void Add(string key, string value)
            {
                d1[key] = value;
                d2[value] = key;
            }
            public void Clear()
            {
                d1.Clear();
                d2.Clear();
            }
            public bool ContainsKey(string key) { return d1.ContainsKey(key); }
            //This api is why we have a 2-sided dictionary. This has the same speed as ContainsKey(). A hash lookup instead of a painful linear search.
            public bool ContainsValue(string value) { return d2.ContainsKey(value); }

            IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() 
            { 
                return d1.GetEnumerator(); 
            }
            IEnumerator IEnumerable.GetEnumerator() 
            {
                return d1.GetEnumerator();
            }

            public bool Remove(string key)
            {
                string value = null;
                if (d1.TryGetValue(key, out value)) d2.Remove(value);
                return d1.Remove(key);
            }
            public bool TryGetValue(string key, out string value) { return d1.TryGetValue(key, out value); }
            public bool TryGetKey(string value, out string key) { return d2.TryGetValue(value, out key); }

            public static implicit operator Dictionary<string,string>(TranslationDictionary tdict)
            {
                return new Dictionary<string,string>(tdict.d1);
            }
            public static implicit operator TranslationDictionary(Dictionary<string,string> dict)
            {
                TranslationDictionary tdict = new TranslationDictionary(dict.Comparer);
                foreach (var kv in dict) { tdict.Add(kv.Key, kv.Value); }
                return tdict;
            }

            //XmlSerializer does not work on dictionaries
            public void Serialize(string filename)
            {
                XmlWriter xml = null;
                try
                {
                    var settings = new XmlWriterSettings();
                    settings.CloseOutput = true;
                    settings.ConformanceLevel = ConformanceLevel.Document;
                    settings.Encoding = Encoding.UTF8;
                    settings.Indent = true;
                    settings.IndentChars = "  ";
                    settings.WriteEndDocumentOnClose = true;

                    xml = XmlWriter.Create(filename, settings);
                    xml.WriteStartDocument();
                    xml.WriteStartElement("TranslationDictionary");
                    xml.WriteAttributeString("LanguageCode", LanguageCode);
                    //Not used internally. This is for human readability only.
                    var ci = CultureInfo.GetCultureInfo(LanguageCode);
                    xml.WriteAttributeString("LanguageName", ci==null?"English":ci.EnglishName);

                    foreach (var kv in d1)
                    {
                        xml.WriteStartElement("Translation");
                        xml.WriteStartElement("Key");
                        xml.WriteValue(kv.Key);
                        xml.WriteEndElement();
                        xml.WriteStartElement("Value");
                        xml.WriteValue(kv.Value);
                        xml.WriteEndElement();
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();
                    xml.WriteEndDocument();
                }
                finally
                {
                    if (xml != null) { xml.Close(); }
                }
            }
            public static TranslationDictionary Deserialize(string filename)
            {
                XmlReader xml = null;
                TranslationDictionary dict = null;
                try
                {
                    var settings = new XmlReaderSettings();
                    settings.CloseInput = true;
                    settings.ConformanceLevel = ConformanceLevel.Document;
                    settings.IgnoreComments = true;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreWhitespace = true;

                    xml = XmlReader.Create(filename, settings);
                    xml.MoveToContent();
                    if (xml.HasAttributes) //<TranslationDictionary LanguageCode="de">
                    {
                        string languageCode = xml.GetAttribute("LanguageCode");
                        if (!languageCode.IsNullOrEmpty() && dict == null)
                            dict = new TranslationDictionary(languageCode);
                    }

                    string nodeName=string.Empty, key=string.Empty, value=string.Empty;
                    while(xml.Read())
                    {
                        switch (xml.NodeType)
                        {
                        case XmlNodeType.Element:
                            switch (xml.Name)
                            {
                                case "Translation":  key=string.Empty; value=string.Empty; break;
                                case "Key": nodeName=xml.Name; break;
                                case "Value": nodeName=xml.Name; break;
                            }
                            break;
                        case XmlNodeType.Text:
                            switch (nodeName)
                            {
                                case "Key": key=xml.Value; break;
                                case "Value": value=xml.Value; break;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (xml.Name=="Translation")
                                dict.Add(key,value);
                            break;
                        }
                    }
                }
                finally
                {
                    if (xml != null) { xml.Close(); }
                }
                return dict;
            }
        }
        #endregion

        #region String Translation
        private static string Translate(string source, string twoLetterISOLanguageName)
        {
            if (TranslatedCache == null) return source; //no one called Initialize()!
            if (twoLetterISOLanguageName == DefaultLanguage) return source; //we don't translate english into english!
            if (string.IsNullOrWhiteSpace(source)) return source; //nothing to translate!
            if (source.All(c=>((c<'A' || c>'Z') && (c<'a' || c>'z')))) return source; //non-alpha character strings do not need to be translated.

            if (TranslatedCache.ContainsValue(source)) return source; //already translated!
            if (TranslatedCache.ContainsKey(source)) return TranslatedCache[source];
            string result = TranslateGoogle(source, twoLetterISOLanguageName);
            //string result = TranslateBabelFish(source, twoLetterISOLanguageName);  //INCOMPLETE
            TranslatedCache.Add(source, result);
            return result;
        }

        private static string TranslateGoogle(string resource, string twoLetterISOLanguageName)
        {
            //Windows and Google use different language codes for these languages
            switch (twoLetterISOLanguageName)
            {
                case "he": twoLetterISOLanguageName = "iw"; break;   //Hebrew
                case "fil": twoLetterISOLanguageName = "tl"; break;  //Phillipino (Tagalog)
            }

            //https://sites.google.com/site/tomihasa/google-language-codes
            //We cannot detect wether or not the translation ever occured. If Google 
            //actually can translate into this particular language. If it is not translated,
            //Google will just return the source string as the translation. We *can* detect 
            //if the 2-letter language code is valid (e.g. 'xx') because the ResponseHeader
            //[Content-Language] will be set to 'en' instead of the target language.
            string result = resource;
            try
            {
                //See: http://weblog.west-wind.com/posts/2011/Aug/06/Translating-with-Google-Translate-without-API-and-C-Code

                string url = string.Format(@"http://translate.google.com/translate_a/t?client=j&text={0}&hl=en&sl={1}&tl={2}",
                               HttpUtility.UrlEncode(resource), "en", twoLetterISOLanguageName);
                WebClient web = new WebClient();
                web.Headers.Add(HttpRequestHeader.UserAgent, @"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0");
                web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
                web.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
                web.Headers.Add(HttpRequestHeader.AcceptEncoding, "identity");
                web.Headers.Add(HttpRequestHeader.Accept, @"text/plain");
                web.Headers.Remove(HttpRequestHeader.Cookie);
                web.Encoding = Encoding.UTF8;
                string html = web.DownloadString(url);

                //Now parse the result
                //SingleLine = {"sentences":[{"trans":"¿Quién es usted?","orig":"Who Are You?","translit":"","src_translit":""}],"src":"en","server_time":1}
                //MultiLine  = {"sentences":[{"trans":"Sehr geehrte {3} {4},\r\n\r\n","orig":"Dear {3} {4},\r\n\r\n","translit":"","src_translit":""},{"trans":"Ihr Passwort zu von produkt Ihrem Administrator zurückgesetzt wurde.\r\n","orig":"Your password to product has been reset by your administrator.\r\n","translit":"","src_translit":""},{"trans":"Ihr Benutzername und Passwort sind temporäre unten. ","orig":"Your user name and temporary password are below.","translit":"","src_translit":""},{"trans":"Beachten Sie, dass Passwörter und Kleinschreibung. ","orig":"Note that passwords are case sensitive.","translit":"","src_translit":""},{"trans":"Bitte loggen Sie sich ein, um produkt mit diesem Passwort und ändern Sie über den Menübefehl \"Passwort-Management\" unter \"Security\"-Menü.\r\n\r\n","orig":"Please login to product using this password and change it using 'Password Management' menu command under 'Security' menu.\r\n\r\n","translit":"","src_translit":""},{"trans":"Benutzername: {1}\r\n","orig":"User Name: {1}\r\n","translit":"","src_translit":""},{"trans":"Temporäre Passwort: {2}\r\n\r\n","orig":"Temporary Password: {2}\r\n\r\n","translit":"","src_translit":""},{"trans":"Danke.","orig":"Thank you.","translit":"","src_translit":""}],"src":"en","server_time":2}
                result = Regex.Replace(html + "trans\":\"\",\"", ".*?trans\":\"(.*?)\",\".*?", "$1", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(\\r|\\n|\\t|\\"")", delegate(Match m)
                {
                    switch (m.Value)
                    {
                        case @"\r": return "\r";
                        case @"\n": return "\n";
                        case @"\t": return "\t";
                        case @"\""": return "\"";
                    }
                    return m.Value;
                }, RegexOptions.None);
            }
            catch (Exception ex)
            {
                LOG.LogError(ex,"Language.GoogleTranslate(\"{0}\", \"{1}\")", resource, twoLetterISOLanguageName);
                return resource;
            }
            LOG.LogInformation("Language.GoogleTranslate(\"{0}\", \"{1}\"): {2}", resource, twoLetterISOLanguageName, result);
            return result;
        }
        #region Dictionary<string, string> GoogleLanguageCodes
        private static readonly Dictionary<string, string> GoogleLanguageCodes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "af", "Afrikaans" },
            { "ar", "Arabic" },
            { "az", "Azerbaijani" },
            { "be", "Belarusian" },
            { "bg", "Bulgarian" },
            { "bn", "Bengali" },
            { "bs", "Bosnian" },
            { "ca", "Catalan" },
            { "ceb", "Cebuano" },
            { "cs", "Czech" },
            { "cy", "Welsh" },
            { "da", "Danish" },
            { "de", "German" },
            { "el", "Greek" },
            { "en", "English" },
            { "eo", "Esperanto" },
            { "es", "Spanish" },
            { "et", "Estonian" },
            { "eu", "Basque" },
            { "fa", "Persian" },
            { "fi", "Finnish" },
            { "fr", "French" },
            { "ga", "Irish" },
            { "gl", "Galician" },
            { "gu", "Gujarati" },
            { "ha", "Hausa" },
            { "hi", "Hindi" },
            { "hmn", "Hmong" },
            { "hr", "Croatian" },
            { "ht", "Haitian Creole" },
            { "hu", "Hungarian" },
            { "hy", "Armenian" },
            { "id", "Indonesian" },
            { "ig", "Igbo" },
            { "is", "Icelandic" },
            { "it", "Italian" },
            { "he", "Hebrew" }, //{ "iw", "Hebrew" }, //use windows language code
            { "ja", "Japanese" },
            { "jw", "Javanese" },
            { "ka", "Georgian" },
            { "km", "Khmer" },
            { "kn", "Kannada" },
            { "ko", "Korean" },
            { "la", "Latin" },
            { "lo", "Lao" },
            { "lt", "Lithuanian" },
            { "lv", "Latvian" },
            { "mi", "Maori" },
            { "mk", "Macedonian" },
            { "mn", "Mongolian" },
            { "mr", "Marathi" },
            { "ms", "Malay" },
            { "mt", "Maltese" },
            { "ne", "Nepali" },
            { "nl", "Dutch" },
            { "no", "Norwegian" },
            { "pa", "Punjabi" },
            { "pl", "Polish" },
            { "pt", "Portuguese" },
            { "ro", "Romanian" },
            { "ru", "Russian" },
            { "sk", "Slovak" },
            { "sl", "Slovenian" },
            { "so", "Somali" },
            { "sq", "Albanian" },
            { "sr", "Serbian" },
            { "sv", "Swedish" },
            { "sw", "Swahili" },
            { "ta", "Tamil" },
            { "te", "Telugu" },
            { "th", "Thai" },
            { "fil", "Filipino" }, //{ "tl", "Filipino" }, //use windows language code
            { "tr", "Turkish" },
            { "uk", "Ukrainian" },
            { "ur", "Urdu" },
            { "vi", "Vietnamese" },
            { "yi", "Yiddish" },
            { "yo", "Yoruba" },
            { "zh", "Chinese" },
            //{ "zh-CN", "Chinese" },
            //{ "zh-TW", "Chinese (Traditional)" },
            { "zu", "Zulu" }
        };
        #endregion //Google Language codes

        private static string RespToRequestCookie(string respCookie)
        {
            //__cfduid=d6e9ac427c1570b7501c2fe1086e9df821411443050622; expires=Mon, 23-Dec-2019 23:50:00 GMT; path=/; domain=.babelfish.com; HttpOnly,PHPSESSID=5d6a15329b86ef92c8a15709e455bfa1; path=/,skip_contest=1; expires=Wed, 24-Sep-2014 00:00:00 GMT; path=/; domain=.babelfish.com
            //Cookie: __cfduid=d60b672faf53f1d9010119b59932b3bcc1411441966638; PHPSESSID=a96415d0d5eadad8e6fffb1282bb0c83; skip_contest=1

            var sb = new StringBuilder();
            char prev_c = '\0';
            bool skip = false;
            foreach (char c in respCookie)
            {
                if (skip && prev_c == ',' && c != ' ') skip = false;
                if (!skip) sb.Append(c);
                prev_c = c;
                if (!skip && c == ';')
                {
                    sb.Append(' ');
                    skip = true;
                }
            }
            sb.Length -= 2;
            return sb.ToString();
        }
        private static string BabelfishCookie = string.Empty;
        private static string TranslateBabelFish(string resource, string twoLetterISOLanguageName)
        {
            // *** INCOMPLETE ***
            string result = resource;

            try
            {
                //See: http://weblog.west-wind.com/posts/2011/Aug/06/Translating-with-Google-Translate-without-API-and-C-Code
                string url;
                WebClient web;

                if (string.IsNullOrWhiteSpace(BabelfishCookie))
                {
                    web = new WebClient();
                    url = @"http://www.babelfish.com/";
                    web.Headers.Add(HttpRequestHeader.UserAgent, @"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0");
                    web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
                    web.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
                    web.Headers.Add(HttpRequestHeader.AcceptEncoding, "identity");
                    web.Headers.Add(HttpRequestHeader.Accept, @"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    web.Headers.Remove(HttpRequestHeader.Cookie);
                    web.DownloadString(url);
                    BabelfishCookie = RespToRequestCookie(web.ResponseHeaders["Set-Cookie"]);
                }

                url = @"http://www.babelfish.com/tools/translate_files/ajax/session.php";
                string post = string.Format(@"act=save_session&lang_s=en&lang_d={1}&phrase={0}", HttpUtility.UrlEncode(resource), twoLetterISOLanguageName);

                web = new WebClient();
                web.Headers.Add(HttpRequestHeader.UserAgent, @"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0");
                web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
                web.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
                web.Headers.Add(HttpRequestHeader.AcceptEncoding, "identity");
                //web.Headers.Add(HttpRequestHeader.Accept, @"text/plain");
                web.Headers.Add(HttpRequestHeader.Cookie, BabelfishCookie);
                web.Encoding = Encoding.UTF8;
                web.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string html = web.UploadString(url, post);
                //{"status":true,"message":"Session has been saved.","cookie_id":false,"session":{"lang_s":"en","lang_d":"fr","phrase":"Help me!"}}

                //Now parse the result
                // <div id="result"><div style="padding:0.6em;">Hallo</div></div>


                result = HttpUtility.HtmlDecode(result);
                LOG.LogInformation("Language.GoogleTranslate(\"{0}\", \"{1}\"): {2}", resource, twoLetterISOLanguageName, result);
                return result;
            }
            catch (Exception ex)
            {
                LOG.LogError(ex, "Language.BabelfishTranslate(\"{0}\", \"{1}\")", resource, twoLetterISOLanguageName);
                return resource;
            }
        }
        #endregion

        #region Translate Windows.Forms Control Labels
        //A control may be a child of many other controls. Keeping the IntPtr handle is a nice way to detect 
        //recursion without interfering with GC. See ActiveHookProc():WM_DESTROY for removal/cleanup.
        private static HashSet<int> TranslatedControls = new HashSet<int>();

        private static void TranslateControl(Control ctrl)
        {
            if (CurrentLanguage == DefaultLanguage) return;

            try
            {
                if (ctrl.IsHandleCreated)
                {
                    if (TranslatedControls.Contains(ctrl.Handle.ToInt32())) return; //already translated!
                    TranslatedControls.Add(ctrl.Handle.ToInt32());
                }

                //do not translate content of editable controls.
                if (ctrl is TextBoxBase) return;
                if (ctrl is ListControl) return;
                if (ctrl is DateTimePicker) return;
                if (ctrl is UpDownBase) return;
                if (ctrl is ElementHost) { TranslateWPFControls((ElementHost)ctrl); return; }
                //if (ctrl is Microsoft.Reporting.WinForms.ReportViewer) { TranslateReportViewerControl(ctrl); return; }
                if (ctrl.GetType().FullName == "Microsoft.Reporting.WinForms.ReportViewer") { TranslateReportViewerControl(ctrl); }

        #region Infragistics controls that need special handling
                //Note: Infragistics is a 3rd-party collection of Dll's and is not allowed to be 
                //referenced in this assembly at all, so we have to make any changes by reflection.
                //See CommonExtensions.cs: Object.Is(), Object.GetValue(), Object.SetValue()
                if (ctrl.GetType().Namespace.StartsWith("Infragistics"))
                {
        #region All Infragistics controls used....
                    //Infragistics.Win.FormattedLinkLabel.UltraFormattedLinkLabel, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraButton, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraDropDownButton, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraExpandableGroupBox, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraExpandableGroupBoxPanel, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraFlowLayoutManager, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraGroupBox, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraLabel, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.Misc.UltraPopupControlContainer, Infragistics2.Win.Misc.v10.2
                    //Infragistics.Win.UltraWinChart.UltraChart, Infragistics2.Win.UltraWinChart.v10.2
                    //Infragistics.Win.UltraWinDock.AutoHideControl, Infragistics2.Win.UltraWinDock.v10.2
                    //Infragistics.Win.UltraWinDock.DockableWindow, Infragistics2.Win.UltraWinDock.v10.2
                    //Infragistics.Win.UltraWinDock.UnpinnedTabArea, Infragistics2.Win.UltraWinDock.v10.2
                    //Infragistics.Win.UltraWinDock.WindowDockingArea, Infragistics2.Win.UltraWinDock.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraCheckEditor, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraComboEditor, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraDateTimeEditor, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraOptionSet, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraPictureBox, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraTextEditor, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinEditors.UltraTrackBar, Infragistics2.Win.UltraWinEditors.v10.2
                    //Infragistics.Win.UltraWinExplorerBar.UltraExplorerBar, Infragistics2.Win.UltraWinExplorerBar.v10.2
                    //Infragistics.Win.UltraWinGrid.UltraGrid, Infragistics2.Win.UltraWinGrid.v10.2
                    //Infragistics.Win.UltraWinListView.UltraListView, Infragistics2.Win.UltraWinListView.v10.2
                    //Infragistics.Win.UltraWinSchedule.UltraCalendarCombo, Infragistics2.Win.UltraWinSchedule.v10.2  ==> UltraScheduleControlBase
                    //Infragistics.Win.UltraWinSchedule.UltraDayView, Infragistics2.Win.UltraWinSchedule.v10.2        ==> UltraScheduleControlBase
                    //Infragistics.Win.UltraWinSchedule.UltraMonthViewMulti, Infragistics2.Win.UltraWinSchedule.v10.2 ==> UltraScheduleControlBase
                    //Infragistics.Win.UltraWinTabControl.UltraTabControl, Infragistics2.Win.UltraWinTabControl.v10.2
                    //Infragistics.Win.UltraWinTabControl.UltraTabPageControl, Infragistics2.Win.UltraWinTabControl.v10.2
                    //Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage, Infragistics2.Win.UltraWinTabControl.v10.2
                    //Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea, Infragistics2.Win.UltraWinToolbars.v10.2
                    //Infragistics.Win.UltraWinToolbars.UltraToolbarsManager, Infragistics2.Win.UltraWinToolbars.v10.2
                    //Infragistics.Win.UltraWinToolTip.UltraToolTipManager, Infragistics2.Win.v10.2
                    //Infragistics.Win.UltraWinTree.UltraTree, Infragistics2.Win.UltraWinTree.v10.2
        #endregion

        #region if (ctrl is Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea) ...
                    if (ctrl.ReflectedIs("Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea, Infragistics2.Win.UltraWinToolbars.v10.2")) //Fixup menu labels
                    {
                        //var c = ctrl as Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea;
                        //string toolCaption = c.ToolbarsManager.Tools[0].SharedProps.Caption;
                        //string tabcaption = c.ToolbarsManager.Ribbon.NonInheritedRibbonTabs[0].Caption;
                        //string groupcaption = c.ToolbarsManager.Ribbon.NonInheritedRibbonTabs[0].Groups[0].Caption;
                        //toolCaption = c.ToolbarsManager.Ribbon.NonInheritedRibbonTabs[0].Groups[0].Tools[0].SharedProps.Caption;

                        String text;
                        Object toolbarsManager = ctrl.GetReflectedValue("ToolbarsManager");
                        Object ribbonTabs = toolbarsManager.GetReflectedValue("Ribbon").GetReflectedValue("NonInheritedRibbonTabs");
                        int ribbonTabKount = (int)ribbonTabs.GetReflectedValue("Count");
                        for (int i = 0; i < ribbonTabKount; i++)
                        {
                            Object item = ribbonTabs.GetReflectedValue("Item", i);
                            text = item.GetReflectedValue("Caption") as String;
                            if (!string.IsNullOrWhiteSpace(text))
                                item.SetReflectedValue("Caption", Translate(text.Replace("&", "")));
                            Object groups = item.GetReflectedValue("Groups");
                            int groupKount = (int)groups.GetReflectedValue("Count");
                            for (int j = 0; j < groupKount; j++)
                            {
                                Object group = groups.GetReflectedValue("Item", j);
                                text = group.GetReflectedValue("Caption") as String;
                                if (!string.IsNullOrWhiteSpace(text))
                                    group.SetReflectedValue("Caption", Translate(text.Replace("&", "")));
                            }
                        }

                        Object tools = toolbarsManager.GetReflectedValue("Tools");
                        int toolKount = (int)tools.GetReflectedValue("Count");
                        for (int i = 0; i < toolKount; i++)
                        {
                            Object sharedProps = tools.GetReflectedValue("Item", i).GetReflectedValue("SharedProps");
                            text = sharedProps.GetReflectedValue("Caption") as String;
                            if (!string.IsNullOrWhiteSpace(text))
                                sharedProps.SetReflectedValue("Caption", Translate(text.Replace("&", "")));
                        }
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinEditors.TextEditorControlBase) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinEditors.TextEditorControlBase, Infragistics2.Win.UltraWinEditors.v10.2"))
                    {
                        return; //editable control. not a label. Infragistics Textbox and Combobox
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinSchedule.UltraScheduleControlBase) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinSchedule.UltraScheduleControlBase, Infragistics.Win.UltraWinSchedule.v10.2"))
                    {
                        return; //editable control. not a label. do not translate;
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinEditors.EditorButtonControlBase) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinEditors.EditorButtonControlBase, Infragistics2.Win.v10.2"))
                    {
                        return; //editable control. not a label. do not translate;
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinSchedule.UltraCalendarCombo) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinSchedule.UltraCalendarCombo, Infragistics2.Win.UltraWinSchedule.v10.2"))
                    {
                        return; //editable control. not a label. do not translate;
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinEditors.UltraOptionSet) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinEditors.UltraOptionSet, Infragistics2.Win.UltraWinEditors.v10.2")) //fixup checkbox labels
                    {
                        //var c = ctrl as Infragistics.Win.UltraWinEditors.UltraOptionSet;
                        //string displayText = c.Items[0].DisplayText;

                        String text;
                        Object items = ctrl.GetReflectedValue("Items");
                        int kount = (int)items.GetReflectedValue("Count");
                        for (int i = 0; i < kount; i++)
                        {
                            Object item = items.GetReflectedValue("Item", i);
                            text = item.GetReflectedValue("DisplayText") as String;
                            if (!string.IsNullOrWhiteSpace(text))
                                item.SetReflectedValue("DisplayText", Translate(text.Replace("&", "")));
                        }
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinGrid.UltraGrid) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinGrid.UltraGrid, Infragistics2.Win.UltraWinGrid.v10.2")) //Fixup column heading labels
                    {
                        //var c = ctrl as Infragistics.Win.UltraWinGrid.UltraGrid;
                        //string key = c.DisplayLayout.Bands[0].Columns[0].Key;
                        //string caption = c.DisplayLayout.Bands[0].Columns[0].Header.Caption;
                        //string ttext = c.DisplayLayout.Bands[0].Columns[0].Header.ToolTipText;

                        String text;
                        Object bands = ctrl.GetReflectedValue("DisplayLayout").GetReflectedValue("Bands");
                        int bandKount = (int)bands.GetReflectedValue("Count");
                        for (int i = 0; i < bandKount; i++)
                        {
                            Object band = bands.GetReflectedValue("Item", i);
                            Object columns = band.GetReflectedValue("Columns");
                            int columnKount = (int)columns.GetReflectedValue("Count");
                            for (int j = 0; j < columnKount; j++)
                            {
                                Object column = columns.GetReflectedValue("Item", j);
                                Object header = column.GetReflectedValue("Header");

                                text = header.GetReflectedValue("Caption") as string;
                                if (string.IsNullOrWhiteSpace(text)) text = column.GetReflectedValue("Key") as string;
                                if (!string.IsNullOrWhiteSpace(text))
                                    header.SetReflectedValue("Caption", Translate(text.Replace("&", "")));

                                text = header.GetReflectedValue("ToolTipText") as string;
                                if (!string.IsNullOrWhiteSpace(text))
                                    header.SetReflectedValue("ToolTipText", Translate(text.Replace("&", "")));
                            }
                        }
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinTabControl.UltraTabControl) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinTabControl.UltraTabControl, Infragistics2.Win.UltraWinTabControl.v10.2")) //Fixup tab labels
                    {
                        //var c = ctrl as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                        //string text = c.Tabs[0].Text;
                        //text = c.Tabs[0].ToolTipText;

                        String text;
                        Object tabs = ctrl.GetReflectedValue("Tabs");
                        int tabKount = (int)tabs.GetReflectedValue("Count");
                        for (int i = 0; i < tabKount; i++)
                        {
                            Object tab = tabs.GetReflectedValue("Item", i);

                            text = tab.GetReflectedValue("Text") as String;
                            if (!string.IsNullOrWhiteSpace(text))
                                tab.SetReflectedValue("Text", Translate(text.Replace("&", "")));

                            text = tab.GetReflectedValue("ToolTipText") as String;
                            if (!string.IsNullOrWhiteSpace(text))
                                tab.SetReflectedValue("ToolTipText", Translate(text.Replace("&", "")));
                        }
                    }
        #endregion
        #region else if (ctrl is Infragistics.Win.UltraWinDock.DockControlBase) ...
                    else if (ctrl.ReflectedIs("Infragistics.Win.UltraWinDock.DockControlBase, Infragistics2.Win.UltraWinDock.v10.2")) //Fixup tab labels
                    {
                        //var c = ctrl as Infragistics.Win.UltraWinDock.DockControlBase;
                        //var mgr = c.Owner as Infragistics.Win.UltraWinDock.UltraDockManager;
                        //string text = mgr.DockAreas[0].Text;
                        //string text2 = mgr.DockAreas[0].Panes[0].Text;
                        //Control c2 = ((Infragistics.Win.UltraWinDock.DockableControlPane)mgr.DockAreas[0].Panes[0]).Control;

                        String text;
                        Object dockMgr = ctrl.GetReflectedValue("Owner");
                        Object dockAreas = dockMgr.GetReflectedValue("DockAreas");
                        if (dockAreas != null) //is it type UltraDockManager ?
                        {
                            int daKount = (int)dockAreas.GetReflectedValue("Count");
                            for (int i = 0; i < daKount; i++)
                            {
                                Object dockArea = dockAreas.GetReflectedValue("Item", i);

                                text = dockArea.GetReflectedValue("Text") as String;
                                if (!string.IsNullOrWhiteSpace(text))
                                    dockArea.SetReflectedValue("Text", Translate(text.Replace("&", "")));

                                Object panes = dockArea.GetReflectedValue("Panes");
                                int paneKount = (int)panes.GetReflectedValue("Count");
                                for (int j = 0; j < paneKount; j++)
                                {
                                    Object pane = panes.GetReflectedValue("Item", j);

                                    text = pane.GetReflectedValue("Text") as String;
                                    if (!string.IsNullOrWhiteSpace(text))
                                        pane.SetReflectedValue("Text", Translate(text.Replace("&", "")));

                                    Control paneCtrl = pane.GetReflectedValue("Control") as Control; //is it type DockableControlPane
                                    if (paneCtrl != null) TranslateControl(paneCtrl); //translate orphaned controls
                                }
                            }
                        }
                    }
        #endregion
                }
        #endregion //Infragistics controls that need special handling

                if (!string.IsNullOrWhiteSpace(ctrl.Text))
                    ctrl.Text = Translate(ctrl.Text.Replace("&", "")); //note: unable to handle hotkeys on foreign languages.

                //Translate accessibility description as well.
                if (!string.IsNullOrWhiteSpace(ctrl.AccessibleDescription))
                    ctrl.AccessibleDescription = Translate(ctrl.AccessibleDescription.Replace("&", ""));

                //Handle when child controls are added/removed
                ctrl.ControlAdded += ctrl_ControlAdded;
                ctrl.ControlRemoved += ctrl_ControlRemoved;

                //Handle when the current control label is changed
                ctrl.TextChanged += ctrl_TextChanged;
            }
            catch (Exception ex)
            {
                LOG.LogWarning(ex, "Error Translating {0}", ctrl.GetType().FullName);
            }

            //Repeat for each child control.
            foreach (Control c in ctrl.Controls)
            {
                TranslateControl(c);
            }        
        }

        private static void TranslateReportViewerControl(Control ctrl)
        {
            //Localization: http://www.codeproject.com/Articles/35225/Advanced-Report-Viewer
        #region Via Assembly Reference
            //CancelEventHandler handler = delegate(object sender, CancelEventArgs e)
            //{
            //    var viewer = sender as Microsoft.Reporting.WinForms.ReportViewer;
            //    Microsoft.Reporting.WinForms.LocalReport localReport = viewer.LocalReport;

            //    if (localReport.ReportPath.IsNullOrEmpty()) return;

            //    var parameters = new List<Microsoft.Reporting.WinForms.ReportParameter>();
            //    foreach (ReportParameterInfo rpi in localReport.GetParameters())
            //    {
            //        string name = rpi.Name;
            //        List<string> values = new List<string>();
            //        foreach (string v in rpi.Values) { values.Add(v); }
            //        bool visible = rpi.Visible;
            //        parameters.Add(new ReportParameter(name, values.ToArray(), rpi.Visible));
            //        if (values.Count > 0) //Translate some known properties
            //        {
            //            if (name.EqualsI("PeriodEndDate")) periodEndDate = values[0].Split(' ').FirstOrDefault();
            //            else if (name.EqualsI("PeriodStartDate")) periodStartDate = values[0].Split(' ').FirstOrDefault();
            //            else if (name.EqualsI("ReportTitle")) values[0] = Internationalization.Translate(values[0]);
            //            else if (name.EqualsI("ReportCriteria"))
            //            {
            //                //ReportCriteria = @"Selected Criteria:
            //                //Date Period BETWEEN 10/14/2014 AND 10/14/2014
            //                //                                    
            //                //Report Options:
            //                //Show PHI = True; Expand All = False; Data Display = Show CDCs; My Items Transactions = Transactions/Patient
            //                //";
            //                string v = values[0];
            //                v = values[0].Replace(periodStartDate, "{0}").Replace(periodEndDate, "{1}");
            //                v = Internationalization.Translate(v);
            //                values[0] = v.Replace("{0}", periodStartDate).Replace("{1}", periodEndDate);
            //            }
            //        }

            //    }
            //    List<Object> datasources = new List<object>();
            //    foreach (Object ds in localReport.DataSources) datasources.Add(ds);
            //    string reportPath = localReport.ReportPath;

            //    viewer.Reset();
            //    localReport = viewer.LocalReport;
            //    localReport.ReportPath = TranslateRDLC(reportPath);
            //    foreach (var ds in datasources) localReport.DataSources.Add((Microsoft.Reporting.WinForms.ReportDataSource)ds);
            //    localReport.SetParameters(parameters);
            //};
            //((Microsoft.Reporting.WinForms.ReportViewer)ctrl).RenderingBegin += handler;
        #endregion
        #region Via Reflection
            CancelEventHandler handler = delegate(object viewer, CancelEventArgs e)
            {
                Object localReport = viewer.GetReflectedValue("LocalReport");
                String reportPath = localReport.GetReflectedValue("ReportPath") as String;
                if (reportPath.IsNullOrEmpty()) return;

                //Object messages = viewer.GetReflectedValue("Messages"); //IReportViewerMessages, IReportViewerMessages2 - Translate ReportViewer labels

                Type reportParameterType = Type.GetType("Microsoft.Reporting.WinForms.ReportParameter, " + viewer.GetType().Assembly.FullName, false, false);
                var parameters = (IList)typeof(List<>).MakeGenericType(reportParameterType).GetConstructor(Type.EmptyTypes).Invoke(null);
                Object pColl = localReport.InvokeReflectedMethod("GetParameters");
                IEnumerator enumerator = pColl.InvokeReflectedMethod("GetEnumerator") as IEnumerator;
                string periodEndDate = string.Empty;
                string periodStartDate = string.Empty;
                while (enumerator.MoveNext())
                {
                    string name = enumerator.Current.GetReflectedValue("Name") as string;
                    IEnumerator e2 = enumerator.Current.GetReflectedValue("Values").InvokeReflectedMethod("GetEnumerator") as IEnumerator;
                    List<string> values = new List<string>();
                    while (e2.MoveNext()) { values.Add(e2.Current as string); }
                    bool visible = (bool)enumerator.Current.GetReflectedValue("Visible");

                    if (values.Count > 0) //Translate some known properties
                    {
                        if (name.EqualsI("PeriodEndDate")) periodEndDate = values[0].Split(' ').FirstOrDefault();
                        else if (name.EqualsI("PeriodStartDate")) periodStartDate = values[0].Split(' ').FirstOrDefault();
                        else if (name.EqualsI("ReportTitle")) values[0] = Internationalization.Translate(values[0]);
                        else if (name.EqualsI("ReportCriteria"))
                        {
                            //ReportCriteria = @"Selected Criteria:
                            //Date Period BETWEEN 10/14/2014 AND 10/14/2014
                            //
                            //Report Options:
                            //Show PHI = True; Expand All = False; Data Display = Show CDCs; My Items Transactions = Transactions/Patient
                            //";
                            string v = values[0];
                            v = values[0].Replace(periodStartDate, "{0}").Replace(periodEndDate, "{1}");
                            v = Internationalization.Translate(v);
                            values[0] = v.Replace("{0}", periodStartDate).Replace("{1}", periodEndDate);
                        }
                    }

                    Object rp = reportParameterType.InvokeReflectedMethod(null, name, values.ToArray(), visible);
                    parameters.InvokeReflectedMethod("Add", rp);
                }

                List<Object> datasources = new List<object>();
                Object dss = localReport.GetReflectedValue("DataSources");
                enumerator = dss.InvokeReflectedMethod("GetEnumerator") as IEnumerator;
                while (enumerator.MoveNext()) { datasources.Add(enumerator.Current); }

                viewer.InvokeReflectedMethod("Reset");
                localReport = viewer.GetReflectedValue("LocalReport");

                localReport.SetReflectedValue("ReportPath", TranslateRDLC(reportPath));
                dss = localReport.GetReflectedValue("DataSources");
                foreach (var ds in datasources) dss.InvokeReflectedMethod("Add", ds);
                localReport.InvokeReflectedMethod("SetParameters", parameters);
            };

            ctrl.InvokeReflectedMethod("add_RenderingBegin", handler);
        #endregion
        }

        /// <summary>
        /// Convert RDLC to current language. Translated file is in the same directory as the source file.
        /// It just includes the language code the name. If the RDLC is already translated, the translated
        /// file just returned.
        /// Example:
        ///     Reports\AnomalousUsageByNursingUnit.rdlc ==> Reports\AnomalousUsageByNursingUnit.de.rdlc
        /// </summary>
        /// <param name="path">full path to default RDLC</param>
        /// <returns>full path to translated RDLC or null upon error</returns>
        /// <seealso cref="http://www.codeproject.com/Articles/31657/Localization-of-RDLC-Reports-into-an-Arabic-Locale"/>
        /// <seealso cref="http://www.codeproject.com/Articles/690216/Replacing-the-default-Windows-calendar-with-NET-ho"/>
        private static string TranslateRDLC(string path)
        {
            if (CurrentLanguage == DefaultLanguage) return path; //nothing to do
            if (path.EndsWith("." + CurrentLanguage + Path.GetExtension(path), StringComparison.InvariantCultureIgnoreCase)) return path; //RDLC is a translated RDLC.
            string translatedPath = Path.ChangeExtension(path, "." + CurrentLanguage + Path.GetExtension(path));
            if (File.Exists(translatedPath)) return translatedPath; //Translated RDLC already exists. Return it.

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            foreach (XmlAttribute attr in xdoc.DocumentElement.Attributes)
            {
                string name = (attr.Name == "xmlns" ? "ns" : attr.Name == "xmlns:rd" ? "rd" : attr.Name);
                nsmgr.AddNamespace(name, attr.Value);
            }
            XmlNodeList tnodes = xdoc.DocumentElement.SelectNodes("//ns:Textbox/ns:Value/text()", nsmgr);
            foreach (XmlText txt in tnodes)
            {
                string value = txt.Value;
                if (string.IsNullOrWhiteSpace(value)) continue;
                if (value[0] == '=') //handle string constants in VB code
                {
                    value = Regex.Replace(value, "(\"[ \t\r\n]*)([^\"]+?)?([ \t\r\n]*\")", m =>
                    {
                        return m.Groups[1].Value + Translate(m.Groups[2].Value) + m.Groups[3].Value;
                    }, RegexOptions.Compiled);
                }
                else value = Translate(value);

                txt.Value = value;
            }

            if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
            {
                //http://www.codeproject.com/Articles/31657/Localization-of-RDLC-Reports-into-an-Arabic-Locale
            }
            
            xdoc.Save(translatedPath);
            return translatedPath;
        }

        private static void RemoveTranslateControl(Control ctrl)
        {
            ctrl.ControlAdded -= ctrl_ControlAdded;
            ctrl.ControlRemoved -= ctrl_ControlRemoved;
            ctrl.TextChanged -= ctrl_TextChanged;
            foreach (Control c in ctrl.Controls)
            {
                RemoveTranslateControl(c);
            }        
        }
        private static void ctrl_ControlRemoved(object sender, ControlEventArgs e)
        {
            RemoveTranslateControl(e.Control);
        }
        private static void ctrl_ControlAdded(object sender, ControlEventArgs e)
        {
            TranslateControl(e.Control);
        }
        private static void ctrl_TextChanged(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null) return;
            if (!string.IsNullOrWhiteSpace(ctrl.Text))
            {
                ctrl.TextChanged -= ctrl_TextChanged;
                ctrl.Text = Translate(ctrl.Text.Replace("&", ""));
                ctrl.TextChanged += ctrl_TextChanged;
            }
        }
        #endregion
        
        #region Translate WPF Controls
        private static void TranslateWPFControls(ElementHost host)
        {
            FrameworkElement child = host.Child as FrameworkElement;
            if (child == null) return;
            //child.Loaded += delegate(object sender, RoutedEventArgs e)
            //{
            //    var c2 = sender as FrameworkElement;
            //    var v2 = sender as Visual;
            //    if (v2 == null) return;
            //    List<Visual> VisualElements = new List<Visual>();
            //    EnumVisual(c2, ref VisualElements);
            //    List<DependencyObject> LogicalElements = new List<DependencyObject>();
            //    EnumLogical(sender, ref LogicalElements);
            //    Console.WriteLine();

            //    child.SourceUpdated += delegate(object s1, DataTransferEventArgs e1)
            //    {
            //        Console.WriteLine();
            //    };
                
            //    //Definition: WPF.Content == Control.Text
            //    //var  bind = new System.Windows.Data.Binding("MyProperty");
            //    //bind.Mode = BindingMode.OneWay;
            //    //bind.Converter = new WPFTranslator();
            //};
        }

        static void child_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void EnumVisual(Visual myVisual, ref List<Visual> elements)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual); i++)
            {
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual, i);
                elements.Add(childVisual);
                // Enumerate children of the child visual object.
                EnumVisual(childVisual, ref elements);
            }
        }
        private static void EnumLogical(Object obj, ref List<DependencyObject> elements)
        {
            // Sometimes leaf nodes aren't DependencyObjects (e.g. strings)
            if (!(obj is DependencyObject)) 
                return;

            elements.Add(obj as DependencyObject);

            // Recursive call for each logical child
            foreach (object child in LogicalTreeHelper.GetChildren(obj as DependencyObject))
            {
                EnumLogical(child, ref elements);
            }
       }

        public class WPFTranslator : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!(value is String) && targetType!=typeof(String)) return value;
                return Translate(value as String);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!(value is String) && targetType != typeof(String)) return value;
                throw new NotImplementedException(); //We don't support translating back to english!
            }
        }
        #endregion
#endif
        #endregion
    }
}
