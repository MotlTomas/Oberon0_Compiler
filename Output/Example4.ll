; ModuleID = 'PrintDemo4'
source_filename = "PrintDemo4"
target triple = "x86_64-pc-linux-gnu"

@i = global i64 0
@r = global double 0.000000e+00
@b = global i1 false
@s = global i8* null
@.str = private unnamed_addr constant [15 x i8] c"Hello, Oberon!\00", align 1
@.str.1 = private unnamed_addr constant [21 x i8] c"=== Print Demo 4 ===\00", align 1
@.fmt = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.2 = private unnamed_addr constant [9 x i8] c"String: \00", align 1
@.fmt.3 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.4 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.5 = private unnamed_addr constant [10 x i8] c"Integer: \00", align 1
@.fmt.6 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.7 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.8 = private unnamed_addr constant [7 x i8] c"Real: \00", align 1
@.fmt.9 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.10 = private unnamed_addr constant [5 x i8] c"%lf\0A\00", align 1
@.str.11 = private unnamed_addr constant [10 x i8] c"Boolean: \00", align 1
@.fmt.12 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.13 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.14 = private unnamed_addr constant [11 x i8] c"Counting: \00", align 1
@.fmt.15 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.16 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.17 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.18 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl.19 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.20 = private unnamed_addr constant [18 x i8] c"A few more lines:\00", align 1
@.fmt.21 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.22 = private unnamed_addr constant [7 x i8] c"Line 1\00", align 1
@.fmt.23 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.24 = private unnamed_addr constant [7 x i8] c"Line 2\00", align 1
@.fmt.25 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.26 = private unnamed_addr constant [7 x i8] c"Line 3\00", align 1
@.fmt.27 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.28 = private unnamed_addr constant [6 x i8] c"Done.\00", align 1
@.fmt.29 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define i32 @main() {
entry:
  store i64 42, i64* @i
  store double 2.718280e+00, double* @r
  store i1 true, i1* @b
  store i8* getelementptr inbounds ([15 x i8], [15 x i8]* @.str, i32 0, i32 0), i8** @s
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([21 x i8], [21 x i8]* @.str.1, i32 0, i32 0))
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.3, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.2, i32 0, i32 0))
  %s = load i8*, i8** @s
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.4, i32 0, i32 0), i8* %s)
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.6, i32 0, i32 0), i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.5, i32 0, i32 0))
  %i = load i64, i64* @i
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.7, i32 0, i32 0), i64 %i)
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.9, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.8, i32 0, i32 0))
  %r = load double, double* @r
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.10, i32 0, i32 0), double %r)
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.12, i32 0, i32 0), i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.11, i32 0, i32 0))
  %b = load i1, i1* @b
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.13, i32 0, i32 0), i1 %b)
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.15, i32 0, i32 0), i8* getelementptr inbounds ([11 x i8], [11 x i8]* @.str.14, i32 0, i32 0))
  store i64 1, i64* @i
  br label %while.cond

while.cond:                                       ; preds = %while.body, %entry
  %i1 = load i64, i64* @i
  %cmp = icmp sle i64 %i1, 5
  br i1 %cmp, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %i2 = load i64, i64* @i
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.16, i32 0, i32 0), i64 %i2)
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.18, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.17, i32 0, i32 0))
  %i3 = load i64, i64* @i
  %add = add i64 %i3, 1
  store i64 %add, i64* @i
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.19, i32 0, i32 0))
  %14 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.21, i32 0, i32 0), i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.20, i32 0, i32 0))
  %15 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.23, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.22, i32 0, i32 0))
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.25, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.24, i32 0, i32 0))
  %17 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.27, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.26, i32 0, i32 0))
  %18 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.29, i32 0, i32 0), i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.str.28, i32 0, i32 0))
  ret i32 0
}
