; ModuleID = 'Functions'
source_filename = "Functions"
target triple = "x86_64-pc-windows-msvc"

@a = global i64 0
@b = global i64 0
@result = global i64 0
@r = global double 0.000000e+00
@.str = private unnamed_addr constant [22 x i8] c"=== Function Demo ===\00", align 1
@.fmt = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.1 = private unnamed_addr constant [21 x i8] c"Nested proc result: \00", align 1
@.fmt.2 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.3 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.4 = private unnamed_addr constant [13 x i8] c"Square(7) = \00", align 1
@.fmt.5 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.6 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.7 = private unnamed_addr constant [13 x i8] c"Add(3, 4) = \00", align 1
@.fmt.8 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.9 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.10 = private unnamed_addr constant [14 x i8] c"Before swap: \00", align 1
@.fmt.11 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.12 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.13 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.14 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.15 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.16 = private unnamed_addr constant [13 x i8] c"After swap: \00", align 1
@.fmt.17 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.18 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.19 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.20 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.21 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.22 = private unnamed_addr constant [18 x i8] c"After increment: \00", align 1
@.fmt.23 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.24 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.25 = private unnamed_addr constant [6 x i8] c"5! = \00", align 1
@.fmt.26 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.27 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.28 = private unnamed_addr constant [11 x i8] c"Fib(10) = \00", align 1
@.fmt.29 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.30 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.31 = private unnamed_addr constant [18 x i8] c"Average(7, 13) = \00", align 1
@.fmt.32 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.33 = private unnamed_addr constant [5 x i8] c"%lf\0A\00", align 1
@.str.34 = private unnamed_addr constant [36 x i8] c"Testing nested proc with Outer(5): \00", align 1
@.fmt.35 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define void @PrintHeader() {
entry:
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([22 x i8], [22 x i8]* @.str, i32 0, i32 0))
  ret void
}

define i64 @Square(i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %x = load i64, i64* %x.addr
  %x1 = load i64, i64* %x.addr
  %mul = mul i64 %x, %x1
  ret i64 %mul
}

define i64 @Add(i64, i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %y.addr = alloca i64
  store i64 %1, i64* %y.addr
  %x = load i64, i64* %x.addr
  %y = load i64, i64* %y.addr
  %add = add i64 %x, %y
  ret i64 %add
}

define void @Swap(i64*, i64*) {
entry:
  %temp = alloca i64
  %x = load i64, i64* %0
  store i64 %x, i64* %temp
  %y = load i64, i64* %1
  store i64 %y, i64* %0
  %temp1 = load i64, i64* %temp
  store i64 %temp1, i64* %1
  ret void
}

define void @Increment(i64*) {
entry:
  %n = load i64, i64* %0
  %add = add i64 %n, 1
  store i64 %add, i64* %0
  ret void
}

define i64 @Factorial(i64) {
entry:
  %n.addr = alloca i64
  store i64 %0, i64* %n.addr
  %n = load i64, i64* %n.addr
  %cmp = icmp sle i64 %n, 1
  br i1 %cmp, label %if.then, label %if.else

if.end:                                           ; No predecessors!
  ret i64 0

if.then:                                          ; preds = %entry
  ret i64 1

if.else:                                          ; preds = %entry
  %n1 = load i64, i64* %n.addr
  %n2 = load i64, i64* %n.addr
  %sub = sub i64 %n2, 1
  %1 = call i64 @Factorial(i64 %sub)
  %mul = mul i64 %n1, %1
  ret i64 %mul
}

define i64 @Fib(i64) {
entry:
  %n.addr = alloca i64
  store i64 %0, i64* %n.addr
  %n = load i64, i64* %n.addr
  %cmp = icmp sle i64 %n, 1
  br i1 %cmp, label %if.then, label %if.else

if.end:                                           ; No predecessors!
  ret i64 0

if.then:                                          ; preds = %entry
  %n1 = load i64, i64* %n.addr
  ret i64 %n1

if.else:                                          ; preds = %entry
  %n2 = load i64, i64* %n.addr
  %sub = sub i64 %n2, 1
  %1 = call i64 @Fib(i64 %sub)
  %n3 = load i64, i64* %n.addr
  %sub4 = sub i64 %n3, 2
  %2 = call i64 @Fib(i64 %sub4)
  %add = add i64 %1, %2
  ret i64 %add
}

define double @Average(i64, i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %y.addr = alloca i64
  store i64 %1, i64* %y.addr
  %x = load i64, i64* %x.addr
  %y = load i64, i64* %y.addr
  %add = add i64 %x, %y
  %sitofp = sitofp i64 %add to double
  %fdiv = fdiv double %sitofp, 2.000000e+00
  ret double %fdiv
}

define void @Outer(i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %outerVar = alloca i64
  %x = load i64, i64* %x.addr
  store i64 %x, i64* %outerVar
  call void @Inner(i64* %outerVar)
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.2, i32 0, i32 0), i8* getelementptr inbounds ([21 x i8], [21 x i8]* @.str.1, i32 0, i32 0))
  %outerVar1 = load i64, i64* %outerVar
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.3, i32 0, i32 0), i64 %outerVar1)
  ret void
}

define void @Inner(i64*) {
entry:
  %outerVar.ptr = alloca i64*
  store i64* %0, i64** %outerVar.ptr
  %outerVar.ptr.load = load i64*, i64** %outerVar.ptr
  %outerVar = load i64, i64* %outerVar.ptr.load
  %add = add i64 %outerVar, 10
  %outerVar.ptr.load1 = load i64*, i64** %outerVar.ptr
  store i64 %add, i64* %outerVar.ptr.load1
  ret void
}

define i32 @main() {
entry:
  call void @PrintHeader()
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.5, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.4, i32 0, i32 0))
  %1 = call i64 @Square(i64 7)
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.6, i32 0, i32 0), i64 %1)
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.8, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.7, i32 0, i32 0))
  %4 = call i64 @Add(i64 3, i64 4)
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.9, i32 0, i32 0), i64 %4)
  store i64 10, i64* @a
  store i64 20, i64* @b
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.11, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.10, i32 0, i32 0))
  %a = load i64, i64* @a
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.12, i32 0, i32 0), i64 %a)
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.14, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.13, i32 0, i32 0))
  %b = load i64, i64* @b
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.15, i32 0, i32 0), i64 %b)
  call void @Swap(i64* @a, i64* @b)
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.17, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.16, i32 0, i32 0))
  %a1 = load i64, i64* @a
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.18, i32 0, i32 0), i64 %a1)
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.20, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.19, i32 0, i32 0))
  %b2 = load i64, i64* @b
  %13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.21, i32 0, i32 0), i64 %b2)
  store i64 5, i64* @result
  call void @Increment(i64* @result)
  %14 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.23, i32 0, i32 0), i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.22, i32 0, i32 0))
  %result = load i64, i64* @result
  %15 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.24, i32 0, i32 0), i64 %result)
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.26, i32 0, i32 0), i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.str.25, i32 0, i32 0))
  %17 = call i64 @Factorial(i64 5)
  %18 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.27, i32 0, i32 0), i64 %17)
  %19 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.29, i32 0, i32 0), i8* getelementptr inbounds ([11 x i8], [11 x i8]* @.str.28, i32 0, i32 0))
  %20 = call i64 @Fib(i64 10)
  %21 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.30, i32 0, i32 0), i64 %20)
  %22 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.32, i32 0, i32 0), i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.31, i32 0, i32 0))
  %23 = call double @Average(i64 7, i64 13)
  store double %23, double* @r
  %r = load double, double* @r
  %24 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.33, i32 0, i32 0), double %r)
  %25 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.35, i32 0, i32 0), i8* getelementptr inbounds ([36 x i8], [36 x i8]* @.str.34, i32 0, i32 0))
  %26 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  call void @Outer(i64 15)
  ret i32 0
}
