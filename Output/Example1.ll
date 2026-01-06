; ModuleID = 'BasicTypes'
source_filename = "BasicTypes"
target triple = "x86_64-pc-windows-msvc"

@i = global i64 0
@j = global i64 0
@r = global double 0.000000e+00
@b = global i1 false
@s = global i8* null
@.fmt = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.fmt.1 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.fmt.2 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.fmt.3 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.fmt.4 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.fmt.5 = private unnamed_addr constant [5 x i8] c"%lf\0A\00", align 1
@.fmt.6 = private unnamed_addr constant [5 x i8] c"%lf\0A\00", align 1
@.fmt.7 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str = private unnamed_addr constant [13 x i8] c"Hello World!\00", align 1
@.fmt.8 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.9 = private unnamed_addr constant [8 x i8] c"Value: \00", align 1
@.fmt.10 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.11 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define i32 @main() {
entry:
  store i64 10, i64* @i
  store i64 5, i64* @j
  %i = load i64, i64* @i
  %j = load i64, i64* @j
  %add = add i64 %i, %j
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt, i32 0, i32 0), i64 %add)
  %i1 = load i64, i64* @i
  %j2 = load i64, i64* @j
  %sub = sub i64 %i1, %j2
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.1, i32 0, i32 0), i64 %sub)
  %i3 = load i64, i64* @i
  %j4 = load i64, i64* @j
  %mul = mul i64 %i3, %j4
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.2, i32 0, i32 0), i64 %mul)
  %i5 = load i64, i64* @i
  %j6 = load i64, i64* @j
  %div = sdiv i64 %i5, %j6
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.3, i32 0, i32 0), i64 %div)
  %i7 = load i64, i64* @i
  %j8 = load i64, i64* @j
  %rem = srem i64 %i7, %j8
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.4, i32 0, i32 0), i64 %rem)
  store double 3.500000e+00, double* @r
  %r = load double, double* @r
  %fmul = fmul double %r, 2.000000e+00
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.5, i32 0, i32 0), double %fmul)
  %r9 = load double, double* @r
  %fdiv = fdiv double 1.000000e+01, %r9
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.6, i32 0, i32 0), double %fdiv)
  %i10 = load i64, i64* @i
  %neg = sub i64 0, %i10
  store i64 %neg, i64* @i
  %i11 = load i64, i64* @i
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.7, i32 0, i32 0), i64 %i11)
  store i1 true, i1* @b
  store i1 true, i1* @b
  store i1 true, i1* @b
  store i1 true, i1* @b
  store i1 true, i1* @b
  store i1 true, i1* @b
  store i1 true, i1* @b
  store i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str, i32 0, i32 0), i8** @s
  %s = load i8*, i8** @s
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.8, i32 0, i32 0), i8* %s)
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.10, i32 0, i32 0), i8* getelementptr inbounds ([8 x i8], [8 x i8]* @.str.9, i32 0, i32 0))
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.11, i32 0, i32 0), i64 42)
  ret i32 0
}
