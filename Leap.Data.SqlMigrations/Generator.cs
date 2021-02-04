namespace Leap.Data.SqlMigrations {
    using System;
    using System.Collections.Generic;

    using Leap.Data.SqlMigrations.Model;

    public class Generator {
        public string CreateCode(Difference diff, string migrationNamespace, string migrationName) {
            var builder = new CodeStringBuilder();
            builder.Append("namespace ").Append(migrationNamespace).Append(" {").AppendLine();
            builder.Indent();
            builder.Append("using FluentMigrator;").AppendLine().AppendLine();
            builder.Append("[Migration(").Append(DateTime.Now.ToString("yyyyMMddHHmm")).Append(")]").AppendLine();
            builder.Append("public class ").Append(migrationName).Append(" : Migration {").AppendLine();
            builder.Indent();
            builder.Append("public override void Up() {").AppendLine();
            builder.Indent();
            var upBuilder = new CodeStringBuilder(builder.GetIndent());
            var downBuilder = new CodeStringBuilder(builder.GetIndent());
            ProcessDiff(diff, upBuilder, downBuilder);
            builder.Append(upBuilder).AppendLine();
            builder.Unindent();
            builder.Append("}").AppendLine().AppendLine();
            builder.Append("public override void Down() {").AppendLine();
            builder.Indent();
            builder.Append(downBuilder).AppendLine();
            builder.Unindent();
            builder.Append("}").AppendLine();
            builder.Unindent();
            builder.Append("}").AppendLine();
            builder.Unindent();
            builder.Append("}").AppendLine();
            return builder.ToString();
        }

        private void ProcessDiff(Difference diff, CodeStringBuilder upBuilder, CodeStringBuilder downBuilder) {
            foreach (var table in diff.CreateTables) {
                // easy :-)
                downBuilder.Append("Delete.Table(\"").Append(table.Name).Append("\");").AppendLine();
                
                // hmm
                upBuilder.Append("Create.Table(\"").Append(table.Name).Append("\")").AppendLine();
                upBuilder.Indent();
                upBuilder.Append(".InSchema(\"").Append(table.Schema).Append("\")").AppendLine();
                foreach (var column in table.Columns) {
                    upBuilder.Append(".WithColumn(\"").Append(column.Name).Append("\").").Append(ColumnType(column));
                    if (column.DefaultValue != null) {
                        upBuilder.Append(".WithDefaultValue(").Append(column.DefaultValue.ToString()).Append(")");
                    }

                    upBuilder.Append(".");
                    upBuilder.Append(column.IsNullable ? "Nullable()" : "NotNullable()");
                    upBuilder.AppendLine();
                }

                upBuilder.Append(";");
                upBuilder.Unindent();
            }
        }

        private string ColumnType(Column column) {
            var noSizeTypes = new Dictionary<Type, string> {
                { typeof(bool), "AsBoolean" },
                { typeof(byte), "AsByte" },
                { typeof(DateTime), "AsDateTime2" },
                { typeof(DateTimeOffset), "AsDateTimeOffset" },
                { typeof(Double), "AsDouble" },
                { typeof(Guid), "AsGuid" },
                { typeof(float), "AsFloat" },
                { typeof(Int16), "AsInt16" },
                { typeof(Int32), "AsInt32" },
                { typeof(Int64), "AsInt64" },
            };

            if (noSizeTypes.TryGetValue(column.Type, out var def)) {
                return $"{def}()";
            }

            // TODO support other types;
            return string.Empty;
        }
    }
}