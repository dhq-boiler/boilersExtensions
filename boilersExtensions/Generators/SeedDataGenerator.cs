﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using boilersExtensions.Models;
using Microsoft.CodeAnalysis;
using PropertyInfo = boilersExtensions.Models.PropertyInfo;

namespace boilersExtensions.Generators
{
    /// <summary>
    /// Entity Frameworkのエンティティに対するシードデータを生成するクラス（改善版）
    /// </summary>
    public class SeedDataGenerator
    {
        private readonly RandomDataProvider _randomDataProvider;
        private readonly EnumValueGenerator _enumValueGenerator;
        private readonly StandardPropertyGenerator _standardPropertyGenerator;
        private readonly Dictionary<string, int> _entityKeyCounters;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SeedDataGenerator()
        {
            _randomDataProvider = new RandomDataProvider();
            _enumValueGenerator = new EnumValueGenerator();
            _standardPropertyGenerator = new StandardPropertyGenerator(_randomDataProvider);
            _entityKeyCounters = new Dictionary<string, int>();
        }

        /// <summary>
        /// 指定されたエンティティに対するシードデータを生成します
        /// </summary>
        /// <param name="entities">エンティティ情報のリスト</param>
        /// <param name="config">シードデータ生成の設定</param>
        /// <returns>生成されたシードデータのコード</returns>
        public string GenerateSeedData(List<EntityInfo> entities, SeedDataConfig config)
        {
            // 依存関係を解決して適切な順序でエンティティを処理
            var orderedEntities = ResolveDependencyOrder(entities);

            var sb = new StringBuilder();

            // ランダム変数の宣言を追加
            sb.AppendLine("    // ランダム生成用のインスタンスを定義");
            sb.AppendLine("    var random = new Random();");
            sb.AppendLine();

            foreach (var entity in orderedEntities)
            {
                // 設定で指定された数のシードデータを生成
                var entityConfig = config.GetEntityConfig(entity.Name);
                if (entityConfig == null || !entityConfig.IsSelected || entityConfig.RecordCount <= 0)
                {
                    continue;
                }

                sb.AppendLine($"    // {entity.Name} エンティティのシードデータ");
                sb.AppendLine($"    modelBuilder.Entity<{entity.Name}>().HasData(");

                for (int i = 0; i < entityConfig.RecordCount; i++)
                {
                    sb.AppendLine($"        new {entity.Name}");
                    sb.AppendLine("        {");

                    // プロパティごとに値を生成
                    var propStrings = new List<string>();
                    foreach (var prop in entity.Properties)
                    {
                        // シードデータから除外するプロパティはスキップ
                        if (prop.ExcludeFromSeed)
                        {
                            continue;
                        }

                        // ナビゲーションプロパティはスキップ（HasDataメソッドでは使用できない）
                        if (prop.IsNavigationProperty)
                        {
                            continue;
                        }

                        // コレクションプロパティはスキップ
                        if (prop.IsCollection)
                        {
                            continue;
                        }

                        // EqualityContract プロパティはスキップ (record型で自動生成される)
                        if (prop.Name == "EqualityContract")
                        {
                            continue;
                        }

                        // 読み取り専用プロパティはスキップ
                        if (prop.Symbol != null && prop.Symbol.SetMethod == null)
                        {
                            continue;
                        }

                        // プロパティの値を生成
                        string propValue = GeneratePropertyValue(prop, i, entityConfig);
                        if (propValue != null)
                        {
                            propStrings.Add($"            {prop.Name} = {propValue}");
                        }
                    }

                    sb.AppendLine(string.Join(",\r\n", propStrings));

                    sb.AppendLine("        }" + (i < entityConfig.RecordCount - 1 ? "," : ""));
                }

                sb.AppendLine("    );");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// プロパティに対する値を生成します
        /// </summary>
        private string GeneratePropertyValue(PropertyInfo property, int recordIndex, EntityConfigViewModel entityConfig)
        {
            // プロパティ固有の設定があれば取得
            var propConfig = entityConfig.GetPropertyConfig(property.Name);

            // 主キーの場合
            if (property.IsKey)
            {
                // 自動生成の場合は連番を使用
                if (property.IsAutoGenerated)
                {
                    return null; // データベースが自動生成するので値を指定しない
                }

                // エンティティごとのカウンターを管理
                var entityName = entityConfig.EntityName;
                if (!_entityKeyCounters.ContainsKey(entityName))
                {
                    _entityKeyCounters[entityName] = 1;
                }

                // インデックスに基づいて主キー値を生成
                int keyValue = recordIndex + 1;
                if (propConfig != null && propConfig.UseCustomStrategy)
                {
                    keyValue = propConfig.CustomStartValue + recordIndex;
                }

                // 数値型の主キーの場合
                switch (property.TypeName)
                {
                    case "Int32":
                    case "Int64":
                    case "Int16":
                        return keyValue.ToString();
                    case "Guid":
                        // 決定論的なGUID生成（レコードごとに一意だが予測可能）
                        return $"new Guid(\"{GenerateDeterministicGuid(entityConfig.EntityName, recordIndex)}\")";
                    case "String":
                        // 文字列型の主キーの場合
                        return $"\"{entityConfig.EntityName}_{keyValue}\"";
                    default:
                        return keyValue.ToString();
                }
            }

            // 外部キーの場合
            if (property.IsForeignKey)
            {
                return GenerateForeignKeyValue(property, recordIndex, entityConfig, propConfig);
            }

            // Enum型の場合
            if (property.IsEnum)
            {
                return _enumValueGenerator.GenerateEnumValue(property, recordIndex, propConfig);
            }

            // 標準プロパティの場合
            return _standardPropertyGenerator.GenerateValue(property, recordIndex, propConfig);
        }

        /// <summary>
        /// 外部キーの値を生成します（様々なデータ型に対応）
        /// </summary>
        private string GenerateForeignKeyValue(
            PropertyInfo property,
            int recordIndex,
            EntityConfigViewModel entityConfig,
            PropertyConfigViewModel propConfig)
        {
            // カスタム値が指定されている場合
            if (propConfig != null && propConfig.UseCustomStrategy)
            {
                return propConfig.CustomValue;
            }

            // リレーションシップの設定があれば使用
            var relationConfig = entityConfig.GetRelationshipConfig(property.ForeignKeyTargetEntity);
            if (relationConfig != null)
            {
                // インデックス計算（リレーションシップタイプに応じて）
                int targetIndex;
                switch (relationConfig.Strategy)
                {
                    case RelationshipStrategy.OneToOne:
                        // 1対1: 同じインデックスを参照
                        targetIndex = recordIndex;
                        break;

                    case RelationshipStrategy.ManyToOne:
                        // 多対1: 指定された親エンティティを参照
                        if (relationConfig.ParentRecordCount > 0)
                        {
                            targetIndex = recordIndex % relationConfig.ParentRecordCount;
                        }
                        else
                        {
                            targetIndex = 0; // デフォルト: 最初の親レコード
                        }
                        break;

                    case RelationshipStrategy.OneToMany:
                        // relationConfig.GetParentIdはインデックスから親IDを計算するメソッド
                        string parentId = relationConfig.GetParentId(recordIndex);
                        if (int.TryParse(parentId, out int parentIdInt))
                        {
                            targetIndex = parentIdInt - 1; // 1ベースから0ベースのインデックスに変換
                        }
                        else
                        {
                            targetIndex = 0;
                        }
                        break;

                    case RelationshipStrategy.Custom:
                        // カスタム: 明示的に指定された値を使用
                        if (relationConfig.CustomMapping.ContainsKey(recordIndex))
                        {
                            // カスタムマッピングの値をそのままフォーマットして返す
                            return FormatForeignKeyValue(
                                property,
                                relationConfig.CustomMapping[recordIndex],
                                property.TypeName,
                                property.IsNullable);
                        }
                        // マッピングがない場合はデフォルトインデックス
                        targetIndex = 0;
                        break;

                    default:
                        targetIndex = 0;
                        break;
                }

                // インデックスに基づいて外部キー値を生成（型に応じてフォーマット）
                return GenerateForeignKeyByTypeAndIndex(
                    property,
                    property.TypeName,
                    property.IsNullable,
                    targetIndex,
                    property.ForeignKeyTargetEntity);
            }

            // デフォルト: 型に応じた最初のレコード参照
            return GenerateForeignKeyByTypeAndIndex(
                property,
                property.TypeName,
                property.IsNullable,
                0,  // 最初のレコード
                property.ForeignKeyTargetEntity);
        }

        /// <summary>
        /// 型とインデックスに基づいて外部キー値を生成
        /// </summary>
        private string GenerateForeignKeyByTypeAndIndex(PropertyInfo property, string typeName, bool isNullable, int index, string targetEntity)
        {
            // Nullableの場合、基本型を取得
            string baseTypeName = typeName;
            if (isNullable && typeName == "Nullable" && !string.IsNullOrEmpty(property.UnderlyingTypeName))
            {
                baseTypeName = property.UnderlyingTypeName;
            }

            switch (baseTypeName)
            {
                case "Guid":
                    // 決定論的なGUIDを生成（エンティティ名とインデックスから一意に決まる）
                    return $"new Guid(\"{GenerateDeterministicGuid(targetEntity, index)}\")";

                case "String":
                    // 文字列型の主キー
                    return $"\"{targetEntity}_{index + 1}\"";

                case "Int64":
                case "Long":
                    // Long型の主キー (1-based index)
                    return $"{index + 1}L";

                case "Int16":
                case "Short":
                    // Short型の主キー
                    return $"(short){index + 1}";

                case "Byte":
                    // バイト型の主キー（小さい値のみ対応）
                    if (index < 255)
                    {
                        return $"(byte){index + 1}";
                    }
                    else
                    {
                        return "(byte)1"; // オーバーフロー回避
                    }

                case "Decimal":
                    // Decimal型
                    return $"{index + 1}m";

                case "Double":
                    // Double型
                    return $"{index + 1}.0d";

                case "Single":
                case "Float":
                    // Float型
                    return $"{index + 1}.0f";

                case "DateTime":
                    // 基準日から日数を加算した日付
                    return $"new DateTime(2023, 1, 1).AddDays({index})";

                case "DateTimeOffset":
                    // 基準日から日数を加算した日付
                    return $"new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays({index})";

                case "Int32":
                case "Int":
                default:
                    // 整数型（デフォルト）- 1-based index
                    return $"{index + 1}";
            }
        }

        /// <summary>
        /// 外部キー値をフォーマット（カスタムマッピング用）
        /// </summary>
        private string FormatForeignKeyValue(PropertyInfo property, object value, string typeName, bool isNullable)
        {
            if (value == null)
            {
                return "null";
            }

            // Nullableの場合、基本型を取得
            string baseTypeName = typeName;
            if (isNullable && typeName == "Nullable" && !string.IsNullOrEmpty(property.UnderlyingTypeName))
            {
                baseTypeName = property.UnderlyingTypeName;
            }

            switch (baseTypeName)
            {
                case "Guid":
                    // GUIDが文字列として格納されている可能性があるので対応
                    if (value is Guid guidValue)
                    {
                        return $"new Guid(\"{guidValue}\")";
                    }
                    else if (Guid.TryParse(value.ToString(), out Guid parsedGuid))
                    {
                        return $"new Guid(\"{parsedGuid}\")";
                    }
                    // 変換できなければ新しいGUIDを生成
                    return $"new Guid(\"{Guid.NewGuid()}\")";

                case "String":
                    return $"\"{value}\"";

                case "Int64":
                case "Long":
                    if (long.TryParse(value.ToString(), out long longValue))
                    {
                        return $"{longValue}L";
                    }
                    return "1L";

                case "Int16":
                case "Short":
                    if (short.TryParse(value.ToString(), out short shortValue))
                    {
                        return $"(short){shortValue}";
                    }
                    return "(short)1";

                case "Byte":
                    if (byte.TryParse(value.ToString(), out byte byteValue))
                    {
                        return $"(byte){byteValue}";
                    }
                    return "(byte)1";

                case "Decimal":
                    if (decimal.TryParse(value.ToString(), out decimal decimalValue))
                    {
                        return $"{decimalValue}m";
                    }
                    return "1m";

                case "Double":
                    if (double.TryParse(value.ToString(), out double doubleValue))
                    {
                        return $"{doubleValue}d";
                    }
                    return "1.0d";

                case "Single":
                case "Float":
                    if (float.TryParse(value.ToString(), out float floatValue))
                    {
                        return $"{floatValue}f";
                    }
                    return "1.0f";

                case "DateTime":
                    if (DateTime.TryParse(value.ToString(), out DateTime dateTimeValue))
                    {
                        return $"new DateTime({dateTimeValue.Year}, {dateTimeValue.Month}, {dateTimeValue.Day}, {dateTimeValue.Hour}, {dateTimeValue.Minute}, {dateTimeValue.Second})";
                    }
                    return "new DateTime(2023, 1, 1)";

                case "DateTimeOffset":
                    if (DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset dateTimeOffsetValue))
                    {
                        return $"new DateTimeOffset({dateTimeOffsetValue.Year}, {dateTimeOffsetValue.Month}, {dateTimeOffsetValue.Day}, {dateTimeOffsetValue.Hour}, {dateTimeOffsetValue.Minute}, {dateTimeOffsetValue.Second}, TimeSpan.Zero)";
                    }
                    return "new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)";

                case "Int32":
                case "Int":
                default:
                    if (int.TryParse(value.ToString(), out int intValue))
                    {
                        return intValue.ToString();
                    }
                    return "1";
            }
        }

        /// <summary>
        /// 予測可能なGUID値を生成します
        /// </summary>
        private string GenerateDeterministicGuid(string entityName, int index)
        {
            // エンティティ名とインデックスに基づいて決定論的なGUIDを生成
            var inputString = $"{entityName}_{index}";
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(inputString);
            var hashBytes = md5.ComputeHash(inputBytes);

            return new Guid(hashBytes).ToString();
        }

        /// <summary>
        /// エンティティ間の依存関係を解決し、適切な順序で処理できるようにします
        /// </summary>
        private List<EntityInfo> ResolveDependencyOrder(List<EntityInfo> entities)
        {
            var result = new List<EntityInfo>();
            var visited = new HashSet<string>();
            var processing = new HashSet<string>();

            // 依存関係をマップ
            var dependencyMap = BuildDependencyMap(entities);

            // 深さ優先探索で依存関係を解決
            foreach (var entity in entities)
            {
                if (!visited.Contains(entity.Name))
                {
                    VisitEntity(entity, entities, dependencyMap, visited, processing, result);
                }
            }

            return result;
        }

        /// <summary>
        /// エンティティ間の依存関係をマップします
        /// </summary>
        private Dictionary<string, List<string>> BuildDependencyMap(List<EntityInfo> entities)
        {
            var dependencyMap = new Dictionary<string, List<string>>();

            foreach (var entity in entities)
            {
                var dependencies = new List<string>();

                // 外部キープロパティの依存関係を追加
                foreach (var fkProp in entity.ForeignKeyProperties)
                {
                    if (!string.IsNullOrEmpty(fkProp.ForeignKeyTargetEntity) &&
                        !dependencies.Contains(fkProp.ForeignKeyTargetEntity))
                    {
                        dependencies.Add(fkProp.ForeignKeyTargetEntity);
                    }
                }

                dependencyMap[entity.Name] = dependencies;
            }

            return dependencyMap;
        }

        /// <summary>
        /// 依存関係の解決に使用する再帰関数
        /// </summary>
        private void VisitEntity(
            EntityInfo entity,
            List<EntityInfo> allEntities,
            Dictionary<string, List<string>> dependencyMap,
            HashSet<string> visited,
            HashSet<string> processing,
            List<EntityInfo> result)
        {
            // 循環参照のチェック
            if (processing.Contains(entity.Name))
            {
                throw new InvalidOperationException(
                    $"循環参照が検出されました: {string.Join(" -> ", processing)} -> {entity.Name}");
            }

            // 既に処理済みならスキップ
            if (visited.Contains(entity.Name))
            {
                return;
            }

            processing.Add(entity.Name);

            // 依存しているエンティティを先に処理
            if (dependencyMap.TryGetValue(entity.Name, out var dependencies))
            {
                foreach (var dependencyName in dependencies)
                {
                    var dependency = allEntities.FirstOrDefault(e => e.Name == dependencyName);
                    if (dependency != null)
                    {
                        VisitEntity(dependency, allEntities, dependencyMap, visited, processing, result);
                    }
                }
            }

            processing.Remove(entity.Name);
            visited.Add(entity.Name);
            result.Add(entity);
        }
    }
}