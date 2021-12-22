namespace TildeSql.SqlMigrations {
    using System.Linq;
    using System.Text;

    public class CodeStringBuilder {
        private int indent;

        private readonly StringBuilder builder;

        private bool isStartOfLine;

        public int GetIndent() {
            return this.indent;
        }

        public CodeStringBuilder(int initialIndent = 0) {
            this.builder       = new StringBuilder();
            this.indent        = initialIndent;
            this.isStartOfLine = true;
        }

        public CodeStringBuilder IncreaseIndent() {
            this.indent += 1;
            return this;
        }

        public CodeStringBuilder DecreaseIndent() {
            this.indent -= 1;
            if (this.indent < 0) {
                this.indent = 0;
            }

            return this;
        }

        public CodeStringBuilder Append(CodeStringBuilder codeStringBuilder) {
            this.builder.Append(codeStringBuilder.builder);
            return this;
        }

        public CodeStringBuilder FormatOff() {
            return this.Append("// @formatter:off").NewLine();
        }
        
        public CodeStringBuilder FormatOn() {
            return this.Append("// @formatter:on").NewLine();
        }   

        public CodeStringBuilder Append(string value) {
            if (this.isStartOfLine) {
                this.AppendIndent();
                this.isStartOfLine = false;
            }
            
            this.builder.Append(value);
            return this;
        }

        public CodeStringBuilder NewLine() {
            this.builder.AppendLine();
            this.isStartOfLine = true;
            return this;
        }

        private void AppendIndent() {
            for (var i = 0; i < this.indent; i++) {
                this.Append4Spaces();
            }
        }

        private void Append4Spaces() {
            this.builder.Append(string.Join("", Enumerable.Range(1, 4).Select(i => " ")));
        }

        public StringBuilder Builder => this.builder;

        public override string ToString() {
            return this.builder.ToString();
        }
    }
}