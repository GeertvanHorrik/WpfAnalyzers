﻿namespace WpfAnalyzers.Test.PropertyChanged.WPF1010MutablePublicPropertyShouldNotifyTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1010MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>
    {
        [Test]
        public async Task AutoPropertyExplicitName()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyCallerMemberNameName()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyPropertyChangedEventArgs()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

////            var fixedCode = @"
////using System.ComponentModel;

////public class Foo : INotifyPropertyChanged
////{
////    private int bar;

////    public event PropertyChangedEventHandler PropertyChanged;

////    public int Bar
////    {
////        get
////        {
////            return this.bar;
////        }

////        set
////        {
////            if (value == this.bar)
////            {
////                return;
////            }

////            this.bar = value;
////            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar)));
////        }
////    }

////    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
////    {
////        this.PropertyChanged?.Invoke(this, e);
////    }
////}";

            // Not sure how we want this, asserting no fix for now
            await this.VerifyCSharpFixAsync(testCode, testCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyPropertyChangedEventArgsAndCallerMemberName()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            ////            var fixedCode = @"
            ////using System.ComponentModel;
            ////using System.Runtime.CompilerServices;
            ////public class Foo : INotifyPropertyChanged
            ////{
            ////    private int bar;

            ////    public event PropertyChangedEventHandler PropertyChanged;

            ////    public int Bar
            ////    {
            ////        get
            ////        {
            ////            return this.bar;
            ////        }

            ////        set
            ////        {
            ////            if (value == this.bar)
            ////            {
            ////                return;
            ////            }

            ////            this.bar = value;
            ////            this.OnPropertyChanged();
            ////        }
            ////    }

            ////    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            ////    {
            ////        this.PropertyChanged?.Invoke(this, e);
            ////    }

            ////    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            ////    {
            ////        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ////    }
            ////}";

            // Not sure how we want this, asserting no fix for now
            await this.VerifyCSharpFixAsync(testCode, testCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyPrivateSet()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
     public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; private set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        private set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyInsertCreatedFieldSorted()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int a;
    private int c;

    public event PropertyChangedEventHandler PropertyChanged;

    public int A
    {
        get
        {
            return this.a;
        }

        set
        {
            if (value == this.a)
            {
                return;
            }

            this.a = value;
            this.OnPropertyChanged(nameof(this.A));
        }
    }

    ↓public int B { get; set; }

    public int C
    {
        get
        {
            return this.c;
        }

        set
        {
            if (value == this.c)
            {
                return;
            }

            this.c = value;
            this.OnPropertyChanged(nameof(this.C));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("B");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int a;
    private int b;
    private int c;

    public event PropertyChangedEventHandler PropertyChanged;

    public int A
    {
        get
        {
            return this.a;
        }

        set
        {
            if (value == this.a)
            {
                return;
            }

            this.a = value;
            this.OnPropertyChanged(nameof(this.A));
        }
    }

    public int B
    {
        get
        {
            return this.b;
        }

        set
        {
            if (value == this.b)
            {
                return;
            }

            this.b = value;
            this.OnPropertyChanged(nameof(this.B));
        }
    }

    public int C
    {
        get
        {
            return this.c;
        }

        set
        {
            if (value == this.c)
            {
                return;
            }

            this.c = value;
            this.OnPropertyChanged(nameof(this.C));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyWhenFieldExists()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;
    private int bar_;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar_;
        }

        set
        {
            if (value == this.bar_)
            {
                return;
            }

            this.bar_ = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WithBackingFieldExplicitName()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            this.value = value;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        private set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged(nameof(this.Value));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WithBackingFieldCallerMemberName()
        {
            var testCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            this.value = value;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        private set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WithBackingFieldCallerMemberNameAccessorsOnOneLine()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Value
    {
        get { return this.value; }
        private set { this.value = value; }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        private set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }
    }
}