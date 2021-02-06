namespace Leap.Data.SqlMigrations {
    using System;
    using System.Linq;

    using Leap.Data.SqlMigrations.Model;

    public class Generator {
        public string CreateCode(Difference diff, string migrationNamespace, string migrationName) {
            var builder = new CodeStringBuilder();
            builder.Append("namespace ").Append(migrationNamespace).Append(" {").AppendLine();
            builder.Indent();
            builder.Append("using System;").AppendLine();
            builder.Append("using FluentMigrator;").AppendLine().AppendLine();
            builder.Append("[Migration(").Append(DateTime.Now.ToString("yyyyMMddHHmm")).Append(")]").AppendLine();
            builder.Append("public class ").Append(migrationName).Append(" : Migration {").AppendLine();
            builder.Indent();
            builder.Append("public override void Up() {").AppendLine();
            builder.Indent();
            var upBuilder = new CodeStringBuilder(builder.GetIndent());
            var downBuilder = new CodeStringBuilder(builder.GetIndent());
            ProcessDiff(diff, upBuilder, downBuilder);
            builder.Append(upBuilder).Unindent().AppendLine();
            builder.Append("}").AppendLine().AppendLine();
            builder.Append("public override void Down() {").AppendLine();
            builder.Indent();
            builder.Append(downBuilder).Unindent().AppendLine();
            builder.Append("}").Unindent().AppendLine();
            builder.Append("}").Unindent().AppendLine();
            builder.Append("}").AppendLine();
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
            createBuilder.Append(";").Unindent();
        }

        private static void WriteCreateTable(CodeStringBuilder createBuilder, CodeStringBuilder dropBuilder, Table table) {
            // easy :-)
            dropBuilder.Append("Delete.Table(\"").Append(table.Name).Append("\");").AppendLine();

            // hmm
            createBuilder.Append("Create.Table(\"").Append(table.Name).Append("\")").AppendLine();
            createBuilder.Indent();
            createBuilder.Append(".InSchema(\"").Append(table.Schema).Append("\")").AppendLine();
            foreach (var column in table.Columns) {
                createBuilder.Append(".WithColumn(\"").Append(column.Name).Append("\")");
                AppendColumnSpec(createBuilder, table, column);
                createBuilder.AppendLine();
            }

            createBuilder.Append(";");
            createBuilder.Unindent();
        }

        private static void AppendColumnSpec(CodeStringBuilder builder, Table table, Column column) {
            builder.Append(column.GenerateColumnType());
            if (column.DefaultValue != null) {
                builder.Append(".WithDefaultValue(").Append(column.DefaultValue.ToString()).Append(")");
            }

            // single column primary key
            if (column.IsPrimaryKey && table.Columns.Count(c => c.IsPrimaryKey) == 1) {
                builder.Append(".PrimaryKey()");
            }

            // TODO multiple column primary key

            builder.Append(".");
            builder.Append(column.IsNullable ? "Nullable()" : "NotNullable()");
        }
    }
}