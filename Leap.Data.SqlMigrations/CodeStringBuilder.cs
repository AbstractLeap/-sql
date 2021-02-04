namespace Leap.Data.SqlMigrations {
    using System.Linq;
    using System.Text;

    public class CodeStringBuilder {
        private int indent;

        private StringBuilder builder;

        public int GetIndent() {
            return this.indent;
        }

        public CodeStringBuilder(int initialIndent = 0) {
            this.builder = new StringBuilder();
            this.indent  = initialIndent;
        }

        public CodeStringBuilder Indent() {
            this.indent += 4;
            this.AppendIndent();
            return this;
        }

        public CodeStringBuilder Unindent() {
            this.indent -= 4;
            if (this.indent < 0) {
                this.indent = 0;
            }

            return this;
        }

        public CodeStringBuilder Append(CodeStringBuilder codeStringBuilder) {
            this.builder.Append(codeStringBuilder.builder);
            return this;
        }

        public CodeStringBuilder Append(string value) {
            this.builder.Append(value);
            return this;
        }

        public CodeStringBuilder AppendLine() {
            this.builder.AppendLine();
            this.AppendIndent();
            return this;
        }

        private void AppendIndent() {
            this.builder.Append(string.Join("", Enumerable.Range(1, this.indent).Select(i => " ")));
        }

        public StringBuilder Builder => this.builder;

        public override string ToString() {
            return this.builder.ToString();
        }
    }
}