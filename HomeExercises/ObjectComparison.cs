using System;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace HomeExercises
{
    public static class ObjectComparison
    {
        [Test]
        [Description("Проверка текущего царя")]
        [Category("ToRefactor")]
        public static void CheckCurrentTsar()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();
            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

            actualTsar.ShouldBeEquivalentTo(expectedTsar, opt => opt.Exclude(typeof(Person), "Id"));
        }

        public static EquivalencyAssertionOptions<Person> Exclude(this EquivalencyAssertionOptions<Person> opt,
            Type type, string value)
        {
            return opt.Excluding(o => o.SelectedMemberInfo.Name == value &&
                                      o.SelectedMemberInfo.DeclaringType == type);
        }


        [Test]
        [Description("Альтернативное решение. Какие у него недостатки?")]
        public static void CheckCurrentTsar_WithCustomEquality()
        {
            var actualTsar = TsarRegistry.GetCurrentTsar();
            var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));

            // Какие недостатки у такого подхода?
            // Не сразу понтяно на чем именно валится тест.   
            // При добавлении нового поля придется добавлять новое сравнение.
            Assert.True(AreEqual(actualTsar, expectedTsar));
        }

        private static bool AreEqual(Person actual, Person expected)
        {
            if (actual == expected) return true;
            if (actual == null || expected == null) return false;
            return
                actual.Name == expected.Name
                && actual.Age == expected.Age
                && actual.Height == expected.Height
                && actual.Weight == expected.Weight
                && AreEqual(actual.Parent, expected.Parent);
        }
    }

    public class TsarRegistry
    {
        public static Person GetCurrentTsar()
        {
            return new Person(
                "Ivan IV The Terrible", 54, 170, 70,
                new Person("Vasili III of Russia", 28, 170, 60, null));
        }
    }

    public class Person
    {
        public static int IdCounter;
        public int Age, Height, Weight;
        public int Id;
        public string Name;
        public Person Parent;

        public Person(string name, int age, int height, int weight, Person parent)
        {
            Id = IdCounter++;
            Name = name;
            Age = age;
            Height = height;
            Weight = weight;
            Parent = parent;
        }
    }
}