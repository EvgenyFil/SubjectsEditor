using System;
using System.Globalization;
using System.Data;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SubjectsStorage
{
    public interface ISubjectsStorage
    {
        void PutSubject(Subject subject);
        // TODO: To think about to deprecate this method
        void PutSubjects(IEnumerable<Subject> subjects);
        IEnumerable<Subject> GetAllSubjects();
    }
    // (-) TODO: To think about using structure instead a class
    public class Subject
    {
        private static readonly int MaxNameLength = 25;
        // Note: First two numbers using for a region's code
        private static readonly long MinPassportNumber = 0100000001;
        private static readonly long MaxPassportNumber = 9999999999;

        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Patronymic { get; private set; }
        public long PassportNumber { get; private set; }
        public DateTime Birthday { get; private set; }

        /// Deprecated
        public static Subject CreateSubject(string name, string surname,
            string patronymic, long passportNumber, DateTime birthday)
        {
            throw new NotImplementedException();
            /*
            Subject newSubject = null;
            string errorText = null;
            if (CheckValues(name, surname, patronymic,
                passportNumber, birthday, out errorText))
            {
                return new Subject(name, surname, patronymic,
                    passportNumber, birthday);
            }
            else
            {
                var exception = new ArgumentException();
                exception.Data.Add("Bad input", errorText);
                throw exception;
            }
            */

        }
        /// Deprecated
        public static Subject CreateSubject(string name, string surname,
            string patronymic, string passportNumber, string birthday)
        {
            return new Subject(name, surname, patronymic, passportNumber, birthday);
        }

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

        /// Check new subject's values and add its to error list
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

        private static bool CheckStringValue(string value)
        {
            if ((value.Length < 1) || (value.Length > MaxNameLength))
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
        public static bool TryParse(string str, out Subject subject)
        {
            subject = null;
            var fields = str.Split(';');
            if (fields.Length != 6)
            {
                return false;
            }
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

        // (+) TODO: To make output more readable
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
    
    public static class SubjectsToCsvConverter
    {
        public static string ConvertToCsvString(Subject subj)
        {
            var passportStr = subj.PassportNumber.ToString().Insert(4, "-");
            var birthdayStr = subj.Birthday.ToShortDateString();
            return String.Format("{0};{1};{2};{3};{4};{5}", subj.Surname, subj.Name,
                subj.Patronymic, birthdayStr, passportStr.Substring(0, 4), passportStr);
        }

        /// Deprecated
        public static void SaveToCsv(string path, IEnumerable<Subject> subjects)
        {
            throw new NotImplementedException();
            using (var sw = new StreamWriter(path: path, append: false))
            {
                foreach (var subj in subjects)
                {
                    sw.WriteLine(subj);
                }                
            }
        }
    }

    class SubjectsStorage : ISubjectsStorage, IDisposable
    {
        public SubjectsStorage(string filename)
        {
            try
            {
                _file = new FileInfo(filename);
                _stream = _file.Open(FileMode.OpenOrCreate);
                _sr = new StreamReader(_stream);
                _sw = new StreamWriter(_stream);
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

    class Program
    {

        // TODO: Split Main() into a few methods (in switch clause at least)
        static void Main()
        {
            var path = @"db.txt";
            using (var dataStorage = new SubjectsStorage(path))
            {
                _subjects = new ObservableCollection<Subject>(dataStorage.GetAllSubjects());
                // TODO: To think a little bit more about [0]-element or null
                _subjects.CollectionChanged += (o, args) => dataStorage.PutSubject(args.NewItems[0] as Subject);
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(@"Type '/help' for help or enter some command:");
                    Console.Write(">> ");
                    var inputStr = Console.ReadLine();

                    while (true)
                    {
                        // TODO: Extract method to dictionary with commands
                        // (to think about dictionary<string, Action>)?
                        switch (inputStr)
                        {
                            case @"/exit":
                                Console.WriteLine("Exit");
                                return;
                            case @"/help":
                                WriteHelp();
                                break;
                            case @"/new":
                                ReadAndSaveSubject(dataStorage);
                                break;
                            case @"/show":
                                foreach (var subj in _subjects)
                                {
                                    Console.WriteLine(subj);
                                }
                                break;
                            case @"/save":
                                SaveSubjects(_subjects);
                                break;
                            case @"/save sort":
                                SortAndSaveSubjects();
                                break;
                            default:
                                break;
                        }
                        Console.Write(">> ");
                        inputStr = Console.ReadLine();
                    }

                }
                while (true);
            }
        }

        private static void SaveSubjects(IEnumerable<Subject> subjects)
        {
            Console.WriteLine("Enter filename: ");
            var filename = Console.ReadLine();
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(path: filename, append: false);
                foreach (var subj in subjects)
                {
                    sw.WriteLine(SubjectsToCsvConverter.ConvertToCsvString(subj));
                }
                sw.Close();
            }
            catch (System.Exception)
            {
                Console.WriteLine("Can't create the file");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }

        }
        private static void SortAndSaveSubjects()
        {
            // TODO: Realize sorting by different fields and orders
            /*
            Console.WriteLine("Enter order (asc/desc: )");
            var order = Console.ReadLine();
            if ((order != "asc") || (order != "desc"))
            {
                return;
            }
            Console.WriteLine("Enter field to sort by (name, surname, birtday):");
            */
            SaveSubjects(_subjects.OrderBy(s => s.Surname)
                                .ThenBy(s => s.Name)
                                .ThenBy(s => s.Patronymic)
                                .ThenBy(s => s.Birthday));
        }
        private static void ReadAndSaveSubject(SubjectsStorage dataStorage)
        {
            string name, surname, patronymic, passportNumber, birthday;
            Console.WriteLine(@"enter name: ");
            name = Console.ReadLine();
            Console.WriteLine(@"enter surname: ");
            surname = Console.ReadLine();
            Console.WriteLine(@"enter patronymic: ");
            patronymic = Console.ReadLine();
            Console.WriteLine(@"enter passport serial and number (without spaces): ");
            passportNumber = Console.ReadLine();
            Console.WriteLine(@"enter birth day (dd/mm/yyyy): ");
            birthday = Console.ReadLine();
            try
            {
                //Subject nextSubj = Subject.CreateSubject(name, surname, patronymic, passportNumber, birthday);
                //dataStorage.PutSubject(nextSubj);
                Subject nextSubj = new Subject(name, surname, patronymic, passportNumber, birthday);
                _subjects.Add(nextSubj);
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Error: {0}", exc.Data["Bad input"]);
            }
        }

        private static void WriteHelp()
        {
            Console.WriteLine(@"/help - this manual");
            Console.WriteLine(@"/exit - close program");
            Console.WriteLine(@"/new  - add new subject");
            Console.WriteLine(@"/show - show subjects from.. to..");
            Console.WriteLine(@"/save - save subjects to csv");
            Console.WriteLine(@"/save sort - sort subjects and save its to csv");
        }

        private static ObservableCollection<Subject> _subjects;
    }
}