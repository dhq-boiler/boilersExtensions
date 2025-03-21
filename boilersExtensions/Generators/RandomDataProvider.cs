﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace boilersExtensions.Generators
{
    /// <summary>
    /// シードデータ生成用のランダムなデータを提供するクラス
    /// </summary>
    public class RandomDataProvider
    {
        private readonly Random _random;

        // サンプルデータリスト
        private static readonly string[] FirstNames = new[]
        {
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles",
            "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen",
            "Emma", "Olivia", "Noah", "Liam", "Sophia", "Jackson", "Aiden", "Lucas", "Ava", "Mia",
            "Takashi", "Yuki", "Haruto", "Sota", "Yuma", "Sakura", "Miku", "Hina", "Rin", "Yui"
        };

        private static readonly string[] LastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor",
            "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson",
            "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "King", "Wright", "Scott", "Green",
            "Sato", "Suzuki", "Takahashi", "Tanaka", "Watanabe", "Ito", "Yamamoto", "Nakamura", "Kobayashi", "Kato"
        };

        private static readonly string[] EmailDomains = new[]
        {
            "gmail.com", "outlook.com", "yahoo.com", "hotmail.com", "aol.com", "icloud.com",
            "example.com", "company.com", "enterprise.org", "university.edu"
        };

        private static readonly string[] CompanyNames = new[]
        {
            "Acme Corp", "Globex", "Initech", "Umbrella Corp", "Massive Dynamic", "Wayne Enterprises",
            "Stark Industries", "Cyberdyne Systems", "Soylent Corp", "Weyland-Yutani",
            "TechSoft", "BioGen", "Global Dynamics", "OmniCorp", "Aperture Science"
        };

        private static readonly string[] JobTitles = new[]
        {
            "Software Engineer", "Marketing Manager", "Sales Representative", "CEO", "Product Manager",
            "Data Analyst", "HR Specialist", "Finance Director", "Operations Manager", "Research Scientist",
            "UX Designer", "Quality Assurance Specialist", "DevOps Engineer", "Systems Administrator", "Project Lead"
        };

        private static readonly string[] StreetNames = new[]
        {
            "Main St", "Oak Ave", "Maple Dr", "Pine Rd", "Cedar Ln", "Park Ave", "Broadway",
            "Washington St", "Franklin Ave", "Highland Dr", "Sunset Blvd", "Lake View Rd"
        };

        private static readonly string[] Cities = new[]
        {
            "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio",
            "San Diego", "Dallas", "San Jose", "Austin", "Tokyo", "London", "Paris", "Berlin", "Sydney"
        };

        private static readonly string[] States = new[]
        {
            "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA",
            "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ"
        };

        private static readonly string[] LoremIpsumWords = new[]
        {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "do",
            "eiusmod", "tempor", "incididunt", "ut", "labore", "et", "dolore", "magna", "aliqua", "ut",
            "enim", "ad", "minim", "veniam", "quis", "nostrud", "exercitation", "ullamco", "laboris", "nisi",
            "ut", "aliquip", "ex", "ea", "commodo", "consequat", "duis", "aute", "irure", "dolor",
            "in", "reprehenderit", "in", "voluptate", "velit", "esse", "cillum", "dolore", "eu", "fugiat",
            "nulla", "pariatur", "excepteur", "sint", "occaecat", "cupidatat", "non", "proident", "sunt", "in",
            "culpa", "qui", "officia", "deserunt", "mollit", "anim", "id", "est", "laborum"
        };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RandomDataProvider()
        {
            _random = new Random();
        }

        /// <summary>
        /// null値を生成すべきかどうかを判定します（Nullableプロパティ用）
        /// </summary>
        /// <returns>10%の確率でtrueを返します</returns>
        public bool ShouldGenerateNull()
        {
            return _random.NextDouble() < 0.1; // 10%の確率でnull
        }

        /// <summary>
        /// ランダムな整数値を生成します
        /// </summary>
        public int GetRandomInt32(double? min = null, double? max = null)
        {
            int minValue = min.HasValue ? (int)min.Value : 1;
            int maxValue = max.HasValue ? (int)max.Value : 1000;

            return _random.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// ランダムなlong値を生成します
        /// </summary>
        public long GetRandomInt64(long? min = null, long? max = null)
        {
            long minValue = min ?? 1;
            long maxValue = max ?? 1000000;

            // long範囲の乱数生成
            byte[] buffer = new byte[8];
            _random.NextBytes(buffer);
            long longRand = BitConverter.ToInt64(buffer, 0);

            return Math.Abs(longRand % (maxValue - minValue + 1)) + minValue;
        }

        /// <summary>
        /// ランダムなshort値を生成します
        /// </summary>
        public short GetRandomInt16(short? min = null, short? max = null)
        {
            short minValue = min ?? (short)1;
            short maxValue = max ?? (short)1000;

            return (short)_random.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// ランダムなbyte値を生成します
        /// </summary>
        public byte GetRandomByte(byte? min = null, byte? max = null)
        {
            byte minValue = min ?? 0;
            byte maxValue = max ?? 255;

            return (byte)_random.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// ランダムなdouble値を生成します
        /// </summary>
        public double GetRandomDouble(double? min = null, double? max = null)
        {
            double minValue = min ?? 0.0;
            double maxValue = max ?? 1000.0;

            return minValue + (_random.NextDouble() * (maxValue - minValue));
        }

        /// <summary>
        /// ランダムなfloat値を生成します
        /// </summary>
        public float GetRandomSingle(float? min = null, float? max = null)
        {
            float minValue = min ?? 0.0f;
            float maxValue = max ?? 1000.0f;

            return minValue + ((float)_random.NextDouble() * (maxValue - minValue));
        }

        /// <summary>
        /// ランダムなdecimal値を生成します
        /// </summary>
        public decimal GetRandomDecimal(decimal? min = null, decimal? max = null)
        {
            decimal minValue = min ?? 0.0m;
            decimal maxValue = max ?? 1000.0m;

            // decimal用の乱数生成
            int scale = _random.Next(0, 29);
            bool sign = _random.Next(0, 2) == 0;

            byte[] bytes = new byte[16];
            _random.NextBytes(bytes);

            // スケールを設定
            bytes[14] = (byte)(scale | (sign ? 0 : 0x80));
            bytes[15] = 0;

            decimal randomDecimal = new decimal(BitConverter.ToInt32(bytes, 0),
                BitConverter.ToInt32(bytes, 4),
                BitConverter.ToInt32(bytes, 8),
                sign, (byte)scale);

            // 範囲内に収める
            return Math.Min(maxValue, Math.Max(minValue, Math.Abs(randomDecimal) / 10000m * (maxValue - minValue) + minValue));
        }

        /// <summary>
        /// ランダムなbool値を生成します
        /// </summary>
        public bool GetRandomBoolean()
        {
            return _random.Next(2) == 1;
        }

        /// <summary>
        /// ランダムな日時を生成します
        /// </summary>
        public DateTime GetRandomDateTime(DateTime? minDate = null, DateTime? maxDate = null)
        {
            DateTime min = minDate ?? new DateTime(2000, 1, 1);
            DateTime max = maxDate ?? DateTime.Now;

            long ticks = min.Ticks + (long)((_random.NextDouble() * (max.Ticks - min.Ticks)));
            return new DateTime(ticks);
        }

        /// <summary>
        /// ランダムなDateTimeOffsetを生成します
        /// </summary>
        public DateTimeOffset GetRandomDateTimeOffset(DateTimeOffset? minDate = null, DateTimeOffset? maxDate = null)
        {
            DateTimeOffset min = minDate ?? new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset max = maxDate ?? DateTimeOffset.Now;

            long ticks = min.Ticks + (long)((_random.NextDouble() * (max.Ticks - min.Ticks)));

            // ランダムなオフセットを生成（-12時間から+14時間まで）
            int offsetHours = _random.Next(-12, 15);
            int offsetMinutes = _random.Next(0, 4) * 15; // 0, 15, 30, 45分

            TimeSpan offset = new TimeSpan(offsetHours, offsetMinutes, 0);
            return new DateTimeOffset(ticks, offset);
        }

        /// <summary>
        /// ランダムなTimeSpanを生成します
        /// </summary>
        public TimeSpan GetRandomTimeSpan(TimeSpan? minTime = null, TimeSpan? maxTime = null)
        {
            TimeSpan min = minTime ?? TimeSpan.Zero;
            TimeSpan max = maxTime ?? TimeSpan.FromDays(365);

            long tickRange = max.Ticks - min.Ticks;
            return min + TimeSpan.FromTicks((long)(_random.NextDouble() * tickRange));
        }

        /// <summary>
        /// ランダムなGuidを生成します
        /// </summary>
        public Guid GetRandomGuid()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        /// ランダムな文字を生成します
        /// </summary>
        public char GetRandomChar(bool onlyAscii = true)
        {
            if (onlyAscii)
            {
                // ASCII文字のみ（読みやすさのため）
                return (char)_random.Next(32, 127);
            }
            else
            {
                // 任意の文字（Unicodeを含む）
                return (char)_random.Next(0, 65536);
            }
        }

        /// <summary>
        /// ランダムなバイト配列を生成します
        /// </summary>
        public byte[] GetRandomByteArray(int length)
        {
            byte[] bytes = new byte[length];
            _random.NextBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// ランダムな文字列を生成します
        /// </summary>
        public string GetRandomString(int maxLength, string prefix = null, int? seed = null)
        {
            // 実際の長さを決定（最大長の30%〜100%）
            int actualLength = _random.Next((int)(maxLength * 0.3), maxLength + 1);

            // 強い関連性を持つキーを生成（プレフィックスとシードに基づく）
            string key = prefix;
            if (seed.HasValue)
            {
                key += "_" + seed.Value;
            }

            // キーがある場合は決定論的に生成
            if (!string.IsNullOrEmpty(key))
            {
                return GeneratePseudoRandomString(key, actualLength);
            }

            // ない場合は完全にランダムに生成
            return GenerateRandomString(actualLength);
        }

        /// <summary>
        /// 完全にランダムな文字列を生成します
        /// </summary>
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 擬似ランダムだが決定論的な文字列を生成します（同じキーに対して常に同じ文字列を返す）
        /// </summary>
        private string GeneratePseudoRandomString(string key, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder(length);

            // キーからハッシュ値を生成
            int hashCode = key.GetHashCode();
            var seededRandom = new Random(hashCode);

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[seededRandom.Next(chars.Length)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// ランダムな名前を生成します
        /// </summary>
        public string GetRandomPersonName()
        {
            return FirstNames[_random.Next(FirstNames.Length)];
        }

        /// <summary>
        /// ランダムな名を生成します
        /// </summary>
        public string GetRandomFirstName()
        {
            return FirstNames[_random.Next(FirstNames.Length)];
        }

        /// <summary>
        /// ランダムな姓を生成します
        /// </summary>
        public string GetRandomLastName()
        {
            return LastNames[_random.Next(LastNames.Length)];
        }

        /// <summary>
        /// ランダムなフルネームを生成します
        /// </summary>
        public string GetRandomFullName()
        {
            return $"{GetRandomFirstName()} {GetRandomLastName()}";
        }

        /// <summary>
        /// ランダムなメールアドレスを生成します
        /// </summary>
        public string GetRandomEmail()
        {
            string name = GetRandomFirstName().ToLower() + _random.Next(1, 1000);
            string domain = EmailDomains[_random.Next(EmailDomains.Length)];
            return $"{name}@{domain}";
        }

        /// <summary>
        /// ランダムな住所を生成します
        /// </summary>
        public string GetRandomAddress()
        {
            string street = $"{_random.Next(1, 9999)} {StreetNames[_random.Next(StreetNames.Length)]}";
            string city = Cities[_random.Next(Cities.Length)];
            string state = States[_random.Next(States.Length)];
            string zip = _random.Next(10000, 99999).ToString();

            return $"{street}, {city}, {state} {zip}";
        }

        /// <summary>
        /// ランダムな電話番号を生成します
        /// </summary>
        public string GetRandomPhoneNumber()
        {
            return $"{_random.Next(100, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}";
        }

        /// <summary>
        /// ランダムなURLを生成します
        /// </summary>
        public string GetRandomUrl()
        {
            string domain = EmailDomains[_random.Next(EmailDomains.Length)];
            return $"https://www.{domain}";
        }

        /// <summary>
        /// ランダムなユーザー名を生成します
        /// </summary>
        public string GetRandomUsername()
        {
            string name = GetRandomFirstName().ToLower();
            return $"{name}{_random.Next(1, 1000)}";
        }

        /// <summary>
        /// ランダムなパスワードを生成します
        /// </summary>
        public string GetRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            int length = _random.Next(8, 16);

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// ランダムな会社名を生成します
        /// </summary>
        public string GetRandomCompanyName()
        {
            return CompanyNames[_random.Next(CompanyNames.Length)];
        }

        /// <summary>
        /// ランダムな役職名を生成します
        /// </summary>
        public string GetRandomJobTitle()
        {
            return JobTitles[_random.Next(JobTitles.Length)];
        }

        /// <summary>
        /// ランダムなLorem Ipsumテキストを生成します
        /// </summary>
        public string GetRandomLoremIpsum(int maxLength)
        {
            var sb = new StringBuilder();

            while (sb.Length < maxLength)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(LoremIpsumWords[_random.Next(LoremIpsumWords.Length)]);
            }

            return sb.ToString().Substring(0, Math.Min(sb.Length, maxLength));
        }
    }
}