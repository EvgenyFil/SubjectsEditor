using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SubjectsEditor.Model
{
    /// <summary>
    /// Access interface for the storage (may hide DBConnector etc.)
    /// </summary>
    public interface ISubjectsStorage
    {
        void PutSubject(Subject subject);
        void PutSubjects(IEnumerable<Subject> subjects);
        IEnumerable<Subject> GetAllSubjects();
    }

    /// <summary>
    /// Represents single person and validation methods
    /// </summary>
    public class Subject
    {
        private static readonly int MaxNameLength = 25;
        // Note: First two numbers using for a region's code
        private static readonly long MinPassportNumber = 0101000001;
        private static readonly long MaxPassportNumber = 9999999999;

        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Patronymic { get; private set; }
        public long PassportNumber { get; private set; }
        public DateTime Birthday { get; private set; }

        /// <summary>
        /// Old verion of constructor (for cmd-app)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="patronymic"></param>
        /// <param name="passportNumberStr"></param>
        /// <param name="birthdayStr"></param>
        public Subject(string name, string surname, string patronymic,
            string passportNumberStr, string birthdayStr)
        {
            var errorMessages = new List<string>(6);
            long passportNumber = 0;
            DateTime birthday = new DateTime();
            try
            {
                passportNumber = long.Parse(passportNumberStr, CultureInfo.InvariantCulture);
            }
            catch (System.Exception)
            {
                errorMessages.Add("Invalid passport number");
            }
            try
            {
                birthday = DateTime.Parse(birthdayStr, CultureInfo.InvariantCulture);
            }
            catch (System.Exception)
            {
                errorMessages.Add("Invalid birth date");
            }
            if (CheckValues(name, surname, patronymic,
                passportNumber, birthday, ref errorMessages))
            {
                Name = name;
                Surname = surname;
                Patronymic = patronymic;
                PassportNumber = passportNumber;
                Birthday = birthday;
                return;
            }
            if (errorMessages.Count != 0)
            {
                var exception = new ArgumentException();
                foreach (var errorMessage in errorMessages)
                {
                    exception.Data.Add("Bad input", errorMessage);
                }
                throw exception;
            }
        }
        /// <summary>
        /// Actual constructor for using in WPF-app
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="patronymic"></param>
        /// <param name="passportNumberStr"></param>
        /// <param name="birthday"></param>
        public Subject(string name, string surname, string patronymic,
            string passportNumberStr, DateTime birthday)
        {
            var errorMessages = new List<string>(6);
            long passportNumber = 0;
            try
            {
                passportNumber = long.Parse(passportNumberStr, CultureInfo.InvariantCulture);
            }
            catch (System.Exception)
            {
                errorMessages.Add("Invalid passport number");
            }
            if (CheckValues(name, surname, patronymic,
                passportNumber, birthday, ref errorMessages))
            {
                Name = name;
                Surname = surname;
                Patronymic = patronymic;
                PassportNumber = passportNumber;
                Birthday = birthday;
                return;
            }
            if (errorMessages.Count != 0)
            {
                var exception = new ArgumentException();
                foreach (var errorMessage in errorMessages)
                {
                    exception.Data.Add("Bad input", errorMessage);
                }
                throw exception;
            }
        }

        /// <summary>
        /// Check arguments and add error messages to ref list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="patronymic"></param>
        /// <param name="passportNumber"></param>
        /// <param name="birthday"></param>
        /// <param name="errorMessages"></param>
        /// <returns></returns>
        private static bool CheckValues(string name, string surname,
            string patronymic, long passportNumber, DateTime birthday,
            ref List<string> errorMessages)
        {
            if (errorMessages == null)
            {
                errorMessages = new List<string>(4);
            }
            if (!CheckStringValue(name))
            {
                errorMessages.Add("Incorrect first name");
            }
            else if (!CheckStringValue(surname))
            {
                errorMessages.Add("Incorrect surname");
            }
            else if ((patronymic != null) && (patronymic.Length > 0) && !CheckStringValue(patronymic))
            {
                errorMessages.Add("Incorrect patronymic");
            }
            else if ((passportNumber < MinPassportNumber) || (passportNumber > MaxPassportNumber))
            {
                errorMessages.Add("Incorrect passport number");
            }

            return errorMessages.Count == 0;
        }

        /// <summary>
        /// Check length and symbols of string values of subject
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool CheckStringValue(string value)
        {
            if ((value == null) || (value.Length < 1) || (value.Length > MaxNameLength))
            {
                return false;
            }
            foreach (var ch in value)
            {
                if (((ch < (int)'А') || (ch > (int)'я'))
                    && ((ch != '-') && (ch != ' ')))
                {
                    return false;
                }
            }
            return true;
        }
        /// Deprecated
        public static bool TryParse(string subjectStr, out Subject subject)
        {
            subject = null;
            var fields = subjectStr.Split(';');
            try
            {
                subject = new Subject(fields[0], fields[1], fields[2],
                    fields[3], fields[4]);
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception in GetData(): {0}", exc.Message);
                return false;
            }
        }

        public static bool CheckName(string name)
        {
            return CheckStringValue(name);
        }

        public static bool CheckSurname(string surname)
        {
            return CheckStringValue(surname);
        }

        public static bool CheckPatronymic(string patronymic)
        {
            if (patronymic == null || patronymic == "")
            {
                return true;
            }
            return CheckStringValue(patronymic);
        }

        public static bool CheckPassportNumber(string passportNumber)
        {
            long number = 0;
            if (!long.TryParse(passportNumber, out number))
            {
                return false;
            }
            return (number >= MinPassportNumber) && (number <= MaxPassportNumber);
        }

        public static bool CheckBirthday(DateTime birthday)
        {
            if (birthday != null && birthday > new DateTime(1900, 1, 1))
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}, pass: {3}, birth: {4}",
                Surname, Name, Patronymic,
                PassportNumber.ToString(CultureInfo.InvariantCulture),
                Birthday.ToString(CultureInfo.InvariantCulture));
            /*
            return String.Format("{0};{1};{2};{3};{4}",
                Name, Surname, Patronymic,
                PassportNumber.ToString(CultureInfo.InvariantCulture),
                Birthday.ToString(CultureInfo.InvariantCulture));
                */
        }
    }

    /// <summary>
    /// Represents saving to csv methods
    /// </summary>
    public static class SubjectsToCsvConverter
    {
        public static string ConvertToCsvString(Subject subj)
        {
            var passportStr = subj.PassportNumber.ToString().Insert(4, "-");
            var birthdayStr = subj.Birthday.ToShortDateString();
            return String.Format("{0};{1};{2};{3};{4};{5}", subj.Surname, subj.Name,
                subj.Patronymic, birthdayStr, passportStr.Substring(0, 4), passportStr);
        }

        public static void SaveToCsv(string path, IEnumerable<Subject> subjects)
        {
            using (var sw = new StreamWriter(path: path, append: false))
            {
                foreach (var subj in subjects)
                {
                    sw.WriteLine(ConvertToCsvString(subj));
                }
            }
        }
    }

    /// <summary>
    /// Concrete storage
    /// </summary>
    class SubjectsStorage : ISubjectsStorage, IDisposable
    {
        public SubjectsStorage(string filename)
        {
            try
            {
                _file = new FileInfo(filename);
                _stream = _file.Open(FileMode.OpenOrCreate);
                _sw = new StreamWriter(_stream);
                _sr = new StreamReader(_stream);
            }
            catch (Exception exc)
            {
                // TODO: 
                Console.WriteLine("Exception while creating DataProvider: {0}", exc.Message);
            }
        }

        public void PutSubject(Subject subject)
        {
            var resultStr = String.Format(@"{0};{1};{2};{3};{4}",
                subject.Name, subject.Surname, subject.Patronymic,
                subject.PassportNumber.ToString(CultureInfo.InvariantCulture),
                subject.Birthday.ToString(CultureInfo.InvariantCulture));
            _sw.WriteLine(resultStr);
            _sw.Flush();
        }

        public void PutSubjects(IEnumerable<Subject> subjects)
        {
            foreach (var subj in subjects)
            {
                _sw.WriteLine(subj);
            }
            _sw.Flush();
        }

        public IEnumerable<Subject> GetAllSubjects()
        {
            var resultSet = new List<Subject>();
            string line = _sr.ReadLine();
            while (line != null)
            {
                var field = line.Split(';');
                if ((field != null) && (field.Length == 5))
                {
                    Subject nextSubject = null;
                    try
                    {
                        /*
                        nextSubject = Subject.CreateSubject(field[0], field[1],
                            field[2], field[3], field[4]);
                            */
                        nextSubject = new Subject(field[0], field[1],
                            field[2], field[3], field[4]);
                        resultSet.Add(nextSubject);
                    }
                    catch (System.Exception)
                    {
                        Console.WriteLine("SubjectsStorage: can't convert string '{0}'",
                            line);
                    }
                }
                line = _sr.ReadLine();
            }
            return resultSet;
        }

        public void Dispose()
        {
            // TODO: ??
            _stream.Flush();
            _stream.Close();
        }

        private StreamReader _sr;
        private StreamWriter _sw;
        private Stream _stream;
        private FileInfo _file;
    }

    public class AppModel
    {
        public ObservableCollection<Subject> Subjects;

        public AppModel()
        {
            Subjects = new ObservableCollection<Subject>(_subjectsStorage.GetAllSubjects());
            Subjects.CollectionChanged += (o, args) => { _subjectsStorage.PutSubject(args.NewItems[0] as Subject); };
        }

        public void SortAndSaveSubjects(string path)
        {
            var sortedSubjects = Subjects.OrderBy(s => s.Surname)
                                            .ThenBy(s => s.Name)
                                            .ThenBy(s => s.Patronymic)
                                            .ThenBy(s => s.Birthday);
            SubjectsToCsvConverter.SaveToCsv(path, sortedSubjects);
        }

        private ISubjectsStorage _subjectsStorage = new SubjectsStorage("db.csv");
    }


}
