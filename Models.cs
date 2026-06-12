using System;

namespace TmsApi.Models
{
    // =========================
    // Enrollment Record
    // =========================
    public record EnrollmentRecord(
        string Id,
        string StudentId,
        string CourseCode,
        DateTime EnrolledAt
    );

    // =========================
    // Course Model
    // =========================
    public class Course
    {
        public required string Code { get; init; }

        private string _title = string.Empty;

        public required string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be empty or whitespace.", nameof(value));

                _title = value;
            }
        }

        private int _capacity;

        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Capacity must be greater than zero.");

                _capacity = value;
            }
        }

        public int EnrolledCount { get; set; }
    }

    // =========================
    // Student Model
    // =========================
    public class Student
    {
        public required string Id { get; init; }

        private string _name = string.Empty;

        public required string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));

                _name = value;
            }
        }

        private int _age;

        public int Age
        {
            get => _age;
            set
            {
                if (value < 16 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Age must be between 16 and 100.");

                _age = value;
            }
        }

        private decimal _gpa;

        public decimal GPA
        {
            get => _gpa;
            set
            {
                if (value < 0.0m || value > 4.0m)
                    throw new ArgumentOutOfRangeException(nameof(value), "GPA must be between 0.0 and 4.0.");

                _gpa = value;
            }
        }
    }

    // =========================
    // Grading Interface
    // =========================
    public interface IGradable
    {
        string Title { get; }
        decimal CalculateGrade();
    }

    // =========================
    // Quiz Implementation
    // =========================
    public class Quiz : IGradable
    {
        public required string Title { get; init; }
        public required int CorrectAnswers { get; init; }
        public required int TotalQuestions { get; init; }

        public decimal CalculateGrade()
        {
            if (TotalQuestions == 0)
                return 0m;

            return (decimal)CorrectAnswers / TotalQuestions * 100m;
        }
    }

    // =========================
    // Lab Assignment Implementation
    // =========================
    public class LabAssignment : IGradable
    {
        public required string Title { get; init; }
        public required decimal FunctionalityScore { get; init; }
        public required decimal CodeQualityScore { get; init; }

        public decimal CalculateGrade()
        {
            // 70% functionality + 30% code quality
            return (FunctionalityScore * 0.7m) +
                   (CodeQualityScore * 0.3m);
        }
    }
}