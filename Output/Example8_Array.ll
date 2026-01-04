; ModuleID = 'ArrayTest'
source_filename = "ArrayTest"
target triple = "x86_64-pc-linux-gnu"

@arr = global [5 x i64] zeroinitializer
@mat = global [3 x [3 x i64]] zeroinitializer
@i = global i64 0
@j = global i64 0
@sum = global i64 0
@.fmt = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.1 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.2 = private unnamed_addr constant [19 x i8] c"=== Array Test ===\00", align 1
@.fmt.3 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.4 = private unnamed_addr constant [17 x i8] c"Filling array...\00", align 1
@.fmt.5 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.6 = private unnamed_addr constant [16 x i8] c"Array contents:\00", align 1
@.fmt.7 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.8 = private unnamed_addr constant [15 x i8] c"Sum of array: \00", align 1
@.fmt.9 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.10 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.nl.11 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.12 = private unnamed_addr constant [16 x i8] c"2D Matrix test:\00", align 1
@.fmt.13 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.fmt.14 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.15 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.16 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl.17 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.18 = private unnamed_addr constant [6 x i8] c"Done!\00", align 1
@.fmt.19 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define void @FillArray([5 x i64]*) {
entry:
  %k = alloca i64
  store i64 0, i64* %k
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %k1 = load i64, i64* %k
  %for.cmp = icmp sle i64 %k1, 4
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %k2 = load i64, i64* %k
  %mul = mul i64 %k2, 10
  %k3 = load i64, i64* %k
  %arrayidx = getelementptr inbounds [5 x i64], [5 x i64]* %0, i64 0, i64 %k3
  store i64 %mul, i64* %arrayidx
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %k4 = load i64, i64* %k
  %for.inc5 = add i64 %k4, 1
  store i64 %for.inc5, i64* %k
  br label %for.cond

for.end:                                          ; preds = %for.cond
  ret void
}

define void @PrintArray([5 x i64]*) {
entry:
  %k = alloca i64
  store i64 0, i64* %k
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %k1 = load i64, i64* %k
  %for.cmp = icmp sle i64 %k1, 4
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %k2 = load i64, i64* %k
  %arrayidx = getelementptr inbounds [5 x i64], [5 x i64]* %0, i64 0, i64 %k2
  %arrayelem = load i64, i64* %arrayidx
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt, i32 0, i32 0), i64 %arrayelem)
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.1, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str, i32 0, i32 0))
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %k3 = load i64, i64* %k
  %for.inc4 = add i64 %k3, 1
  store i64 %for.inc4, i64* %k
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  ret void
}

define i64 @SumArray([5 x i64]*) {
entry:
  %k = alloca i64
  %s = alloca i64
  store i64 0, i64* %s
  store i64 0, i64* %k
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %k1 = load i64, i64* %k
  %for.cmp = icmp sle i64 %k1, 4
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %s2 = load i64, i64* %s
  %k3 = load i64, i64* %k
  %arrayidx = getelementptr inbounds [5 x i64], [5 x i64]* %0, i64 0, i64 %k3
  %arrayelem = load i64, i64* %arrayidx
  %add = add i64 %s2, %arrayelem
  store i64 %add, i64* %s
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %k4 = load i64, i64* %k
  %for.inc5 = add i64 %k4, 1
  store i64 %for.inc5, i64* %k
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %s6 = load i64, i64* %s
  ret i64 %s6
}

define i32 @main() {
entry:
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.3, i32 0, i32 0), i8* getelementptr inbounds ([19 x i8], [19 x i8]* @.str.2, i32 0, i32 0))
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.5, i32 0, i32 0), i8* getelementptr inbounds ([17 x i8], [17 x i8]* @.str.4, i32 0, i32 0))
  call void @FillArray([5 x i64]* @arr)
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.7, i32 0, i32 0), i8* getelementptr inbounds ([16 x i8], [16 x i8]* @.str.6, i32 0, i32 0))
  call void @PrintArray([5 x i64]* @arr)
  %3 = call i64 @SumArray([5 x i64]* @arr)
  store i64 %3, i64* @sum
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.9, i32 0, i32 0), i8* getelementptr inbounds ([15 x i8], [15 x i8]* @.str.8, i32 0, i32 0))
  %sum = load i64, i64* @sum
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.10, i32 0, i32 0), i64 %sum)
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.11, i32 0, i32 0))
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.13, i32 0, i32 0), i8* getelementptr inbounds ([16 x i8], [16 x i8]* @.str.12, i32 0, i32 0))
  store i64 0, i64* @i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i = load i64, i64* @i
  %for.cmp = icmp sle i64 %i, 2
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  store i64 0, i64* @j
  br label %for.cond1

for.inc:                                          ; preds = %for.end4
  %i12 = load i64, i64* @i
  %for.inc13 = add i64 %i12, 1
  store i64 %for.inc13, i64* @i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  store i64 0, i64* @i
  br label %for.cond14

for.cond1:                                        ; preds = %for.inc3, %for.body
  %j = load i64, i64* @j
  %for.cmp5 = icmp sle i64 %j, 2
  br i1 %for.cmp5, label %for.body2, label %for.end4

for.body2:                                        ; preds = %for.cond1
  %i6 = load i64, i64* @i
  %mul = mul i64 %i6, 3
  %j7 = load i64, i64* @j
  %add = add i64 %mul, %j7
  %i8 = load i64, i64* @i
  %j9 = load i64, i64* @j
  %arrayidx = getelementptr inbounds [3 x [3 x i64]], [3 x [3 x i64]]* @mat, i64 0, i64 %i8, i64 %j9
  store i64 %add, i64* %arrayidx
  br label %for.inc3

for.inc3:                                         ; preds = %for.body2
  %j10 = load i64, i64* @j
  %for.inc11 = add i64 %j10, 1
  store i64 %for.inc11, i64* @j
  br label %for.cond1

for.end4:                                         ; preds = %for.cond1
  br label %for.inc

for.cond14:                                       ; preds = %for.inc16, %for.end
  %i18 = load i64, i64* @i
  %for.cmp19 = icmp sle i64 %i18, 2
  br i1 %for.cmp19, label %for.body15, label %for.end17

for.body15:                                       ; preds = %for.cond14
  store i64 0, i64* @j
  br label %for.cond20

for.inc16:                                        ; preds = %for.end23
  %i31 = load i64, i64* @i
  %for.inc32 = add i64 %i31, 1
  store i64 %for.inc32, i64* @i
  br label %for.cond14

for.end17:                                        ; preds = %for.cond14
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.19, i32 0, i32 0), i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.str.18, i32 0, i32 0))
  ret i32 0

for.cond20:                                       ; preds = %for.inc22, %for.body15
  %j24 = load i64, i64* @j
  %for.cmp25 = icmp sle i64 %j24, 2
  br i1 %for.cmp25, label %for.body21, label %for.end23

for.body21:                                       ; preds = %for.cond20
  %i26 = load i64, i64* @i
  %j27 = load i64, i64* @j
  %arrayidx28 = getelementptr inbounds [3 x [3 x i64]], [3 x [3 x i64]]* @mat, i64 0, i64 %i26, i64 %j27
  %arrayelem = load i64, i64* %arrayidx28
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.14, i32 0, i32 0), i64 %arrayelem)
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.16, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.15, i32 0, i32 0))
  br label %for.inc22

for.inc22:                                        ; preds = %for.body21
  %j29 = load i64, i64* @j
  %for.inc30 = add i64 %j29, 1
  store i64 %for.inc30, i64* @j
  br label %for.cond20

for.end23:                                        ; preds = %for.cond20
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.17, i32 0, i32 0))
  br label %for.inc16
}
