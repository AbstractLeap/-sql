namespace Leap.Data.SqlMigrations {
    using System;
    using System.Linq;

    using Leap.Data.SqlMigrations.Model;

    public class Generator {
        public string CreateCode(Difference diff, string migrationNamespace, string migrationName) {
            var builder = new CodeStringBuilder();
            builder.Append("namespace ").Append(migrationNamespace).Append(" {").NewLine();
            builder.IncreaseIndent();
            builder.Append("using System;").NewLine();
            builder.Append("using FluentMigrator;").NewLine().NewLine();
            builder.Append("[Migration(").Append(DateTime.Now.ToString("yyyyMMddHHmm")).Append(")]").NewLine();
            builder.Append("public class ").Append(migrationName).Append(" : Migration {").NewLine();
            builder.IncreaseIndent();
            builder.Append("public override void Up() {").NewLine();
            builder.IncreaseIndent();
            var upBuilder = new CodeStringBuilder(builder.GetIndent());
            var downBuilder = new CodeStringBuilder(builder.GetIndent());
            ProcessDiff(diff, upBuilder, downBuilder);
            builder.FormatOff();
            builder.Append(upBuilder);
            builder.FormatOn();
            builder.DecreaseIndent();
            builder.Append("}").NewLine().NewLine();
            builder.Append("public override void Down() {").NewLine();
            builder.IncreaseIndent();
            builder.FormatOff();
            builder.Append(downBuilder);
            builder.FormatOn();
            builder.DecreaseIndent();
            builder.Append("}").DecreaseIndent().NewLine();
            builder.Append("}").DecreaseIndent().NewLine();
            builder.Append("}").NewLine();
            return builder.ToString();
        }

        private void ProcessDiff(Difference diff, CodeStringBuilder upBuilder, CodeStringBuilder downBuilder) {
            foreach (var table in diff.CreateTables) {
                WriteCreateTable(upBuilder, downBuilder, table);
            }

            foreach (var (table, column) in diff.CreateColumns) {
                WriteAddColumn(upBuilder, downBuilder, column, table);
            }

            foreach (var (table, oldColumn, newColumn, changedProperties) in diff.AlterColumns) {
                throw new NotImplementedException();
            }

            foreach (var (table, column) in diff.DropColumns) {
                WriteAddColumn(downBuilder, upBuilder, column, table);
            }

            foreach (var table in diff.DropTables) {
                WriteCreateTable(downBuilder, upBuilder, table);
            }
        }

        private static void WriteAddColumn(CodeStringBuilder createBuilder, CodeStringBuilder dropBuilder, Column column, Table table) {
            // down is easy
            dropBuilder.Append("Delete.Column(\"").Append(column.Name).Append("\").FromTable(\"").Append(table.Name).Append("\");");

            createBuilder.Append("Alter.Table(\"").Append(table.Name).Append("\").AddColumn(\"").Append(column.Name).Append("\")");
            AppendColumnSpec(createBuilder, table, column);
            createBuilder.Append(";").DecreaseIndent();
        }

        private static void WriteCreateTable(CodeStringBuilder createBuilder, CodeStringBuilder dropBuilder, Table table) {
            // easy :-)
            dropBuilder.Append("Delete.Table(\"").Append(table.Name).Append("\").InSchema(\"").Append(table.Schema).Append("\");").NewLine();

            // hmm
            createBuilder.Append("Create.Table(\"").Append(table.Name).Append("\")").NewLine();
            createBuilder.IncreaseIndent();
            createBuilder.Append(".InSchema(\"").Append(table.Schema).Append("\")").NewLine();
            foreach (var column in table.Columns) {
                createBuilder.Append(".WithColumn(\"").Append(column.Name).Append("\")");
                AppendColumnSpec(createBuilder, table, column);
                createBuilder.NewLine();
            }

            createBuilder.Append(";");
            createBuilder.DecreaseIndent().NewLine();
        }

        private static void AppendColumnSpec(CodeStringBuilder builder, Table table, Column column) {
            builder.Append(column.GenerateColumnType());
            if (column.DefaultValue != null) {
                builder.Append(".WithDefaultValue(").Append(column.DefaultValue.ToString()).Append(")");
            }

            // single column primary key
            if (column.IsPrimaryKey) {
                builder.Append(".PrimaryKey()");
            }

            builder.Append(".");
            builder.Append(column.IsNullable ? "Nullable()" : "NotNullable()");
        }
    }
}