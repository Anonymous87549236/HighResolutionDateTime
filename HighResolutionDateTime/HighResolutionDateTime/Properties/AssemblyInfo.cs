using System.Reflection;
using System.Runtime.InteropServices;

// Общие сведения об этой сборке предоставляются следующим набором
// набора атрибутов. Измените значения этих атрибутов для изменения сведений,
// связанные со сборкой.
[assembly: AssemblyTitle("HighResolutionDateTime")]
[assembly: AssemblyDescription("DateTime and DateTimeOffset based on GetSystemTimePreciseAsFileTime, Stopwatch or DateTime.")]
#if (DEBUG)
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
// [assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("HighResolutionDateTime")]
[assembly: AssemblyCopyright("The MIT License (MIT)")]
// [assembly: AssemblyTrademark("")]
// [assembly: AssemblyCulture("")]

// Установка значения False для параметра ComVisible делает типы в этой сборке невидимыми
// для компонентов COM. Если необходимо обратиться к типу в этой сборке через
// COM, задайте атрибуту ComVisible значение TRUE для этого типа.
[assembly: ComVisible(true)]

// Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: Guid("3cdccf20-52fa-4174-934f-61cceef9d8e6")]

// Сведения о версии сборки состоят из указанных ниже четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии
//      Номер сборки
//      Редакция
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.5.5.0")]
[assembly: AssemblyFileVersion("1.5.5.0")]