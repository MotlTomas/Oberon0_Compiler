; ModuleID = 'Arrays'
source_filename = "Arrays"
target triple = "x86_64-pc-windows-msvc"

@v = global [5 x i64] zeroinitializer
@m = global [3 x [3 x i64]] zeroinitializer
@copy = global [5 x i64] zeroinitializer
@i = global i64 0
@j = global i64 0
@sum = global i64 0
@.str = private unnamed_addr constant [9 x i8] c"Vector: \00", align 1
@.fmt = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.1 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.2 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.3 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.4 = private unnamed_addr constant [6 x i8] c"Sum: \00", align 1
@.fmt.5 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.6 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.7 = private unnamed_addr constant [16 x i8] c"Original v[0]: \00", align 1
@.fmt.8 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.9 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.10 = private unnamed_addr constant [10 x i8] c"Copy[0]: \00", align 1
@.fmt.11 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.12 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.13 = private unnamed_addr constant [8 x i8] c"Matrix:\00", align 1
@.fmt.14 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.fmt.15 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.16 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.17 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl.18 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.19 = private unnamed_addr constant [15 x i8] c"Diagonal sum: \00", align 1
@.fmt.20 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.21 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.22 = private unnamed_addr constant [18 x i8] c"Modified m[1,1]: \00", align 1
@.fmt.23 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.24 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define void @FillVector([5 x i64]*) {
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
  %k3 = load i64, i64* %k
  %mul = mul i64 %k3, 10
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

define i64 @SumVector([5 x i64]*) {
entry:
  %k = alloca i64
  %total = alloca i64
  store i64 0, i64* %total
  store i64 0, i64* %k
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %k1 = load i64, i64* %k
  %for.cmp = icmp sle i64 %k1, 4
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %total2 = load i64, i64* %total
  %k3 = load i64, i64* %k
  %arrayidx = getelementptr inbounds [5 x i64], [5 x i64]* %0, i64 0, i64 %k3
  %elem = load i64, i64* %arrayidx
  %add = add i64 %total2, %elem
  store i64 %add, i64* %total
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %k4 = load i64, i64* %k
  %for.inc5 = add i64 %k4, 1
  store i64 %for.inc5, i64* %k
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %total6 = load i64, i64* %total
  ret i64 %total6
}

define void @FillMatrix([3 x [3 x i64]]*) {
entry:
  %r = alloca i64
  %c = alloca i64
  store i64 0, i64* %r
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %r1 = load i64, i64* %r
  %for.cmp = icmp sle i64 %r1, 2
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  store i64 0, i64* %c
  br label %for.cond2

for.inc:                                          ; preds = %for.end5
  %r15 = load i64, i64* %r
  %for.inc16 = add i64 %r15, 1
  store i64 %for.inc16, i64* %r
  br label %for.cond

for.end:                                          ; preds = %for.cond
  ret void

for.cond2:                                        ; preds = %for.inc4, %for.body
  %c6 = load i64, i64* %c
  %for.cmp7 = icmp sle i64 %c6, 2
  br i1 %for.cmp7, label %for.body3, label %for.end5

for.body3:                                        ; preds = %for.cond2
  %r8 = load i64, i64* %r
  %c9 = load i64, i64* %c
  %arrayidx = getelementptr inbounds [3 x [3 x i64]], [3 x [3 x i64]]* %0, i64 0, i64 %r8, i64 %c9
  %r10 = load i64, i64* %r
  %mul = mul i64 %r10, 3
  %c11 = load i64, i64* %c
  %add = add i64 %mul, %c11
  %add12 = add i64 %add, 1
  store i64 %add12, i64* %arrayidx
  br label %for.inc4

for.inc4:                                         ; preds = %for.body3
  %c13 = load i64, i64* %c
  %for.inc14 = add i64 %c13, 1
  store i64 %for.inc14, i64* %c
  br label %for.cond2

for.end5:                                         ; preds = %for.cond2
  br label %for.inc
}

define i64 @DiagonalSum([3 x [3 x i64]]*) {
entry:
  %k = alloca i64
  %total = alloca i64
  store i64 0, i64* %total
  store i64 0, i64* %k
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %k1 = load i64, i64* %k
  %for.cmp = icmp sle i64 %k1, 2
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %total2 = load i64, i64* %total
  %k3 = load i64, i64* %k
  %k4 = load i64, i64* %k
  %arrayidx = getelementptr inbounds [3 x [3 x i64]], [3 x [3 x i64]]* %0, i64 0, i64 %k3, i64 %k4
  %elem = load i64, i64* %arrayidx
  %add = add i64 %total2, %elem
  store i64 %add, i64* %total
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %k5 = load i64, i64* %k
  %for.inc6 = add i64 %k5, 1
  store i64 %for.inc6, i64* %k
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %total7 = load i64, i64* %total
  ret i64 %total7
}

define i32 @main() {
entry:
  call void @FillVector([5 x i64]* @v)
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str, i32 0, i32 0))
  store i64 0, i64* @i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i = load i64, i64* @i
  %for.cmp = icmp sle i64 %i, 4
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %i1 = load i64, i64* @i
  %arrayidx = getelementptr inbounds [5 x i64], [5 x i64]* @v, i64 0, i64 %i1
  %elem = load i64, i64* %arrayidx
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.1, i32 0, i32 0), i64 %elem)
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.3, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.2, i32 0, i32 0))
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %i2 = load i64, i64* @i
  %for.inc3 = add i64 %i2, 1
  store i64 %for.inc3, i64* @i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.5, i32 0, i32 0), i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.str.4, i32 0, i32 0))
  %5 = call i64 @SumVector([5 x i64]* @v)
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.6, i32 0, i32 0), i64 %5)
  %v = load [5 x i64], [5 x i64]* @v
  %7 = call i8* @memcpy(i8* bitcast ([5 x i64]* @copy to i8*), i8* bitcast ([5 x i64]* @v to i8*), i64 40)
  store i64 999, i64* getelementptr inbounds ([5 x i64], [5 x i64]* @copy, i64 0, i64 0)
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.8, i32 0, i32 0), i8* getelementptr inbounds ([16 x i8], [16 x i8]* @.str.7, i32 0, i32 0))
  %elem4 = load i64, i64* getelementptr inbounds ([5 x i64], [5 x i64]* @v, i64 0, i64 0)
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.9, i32 0, i32 0), i64 %elem4)
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.11, i32 0, i32 0), i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.10, i32 0, i32 0))
  %elem5 = load i64, i64* getelementptr inbounds ([5 x i64], [5 x i64]* @copy, i64 0, i64 0)
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.12, i32 0, i32 0), i64 %elem5)
  call void @FillMatrix([3 x [3 x i64]]* @m)
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.14, i32 0, i32 0), i8* getelementptr inbounds ([8 x i8], [8 x i8]* @.str.13, i32 0, i32 0))
  store i64 0, i64* @i
  br label %for.cond6

for.cond6:                                        ; preds = %for.inc8, %for.end
  %i10 = load i64, i64* @i
  %for.cmp11 = icmp sle i64 %i10, 2
  br i1 %for.cmp11, label %for.body7, label %for.end9

for.body7:                                        ; preds = %for.cond6
  store i64 0, i64* @j
  br label %for.cond12

for.inc8:                                         ; preds = %for.end15
  %i23 = load i64, i64* @i
  %for.inc24 = add i64 %i23, 1
  store i64 %for.inc24, i64* @i
  br label %for.cond6

for.end9:                                         ; preds = %for.cond6
  %13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.20, i32 0, i32 0), i8* getelementptr inbounds ([15 x i8], [15 x i8]* @.str.19, i32 0, i32 0))
  %14 = call i64 @DiagonalSum([3 x [3 x i64]]* @m)
  %15 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.21, i32 0, i32 0), i64 %14)
  store i64 100, i64* getelementptr inbounds ([3 x [3 x i64]], [3 x [3 x i64]]* @m, i64 0, i64 1, i64 1)
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.23, i32 0, i32 0), i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.22, i32 0, i32 0))
  %elem25 = load i64, i64* getelementptr inbounds ([3 x [3 x i64]], [3 x [3 x i64]]* @m, i64 0, i64 1, i64 1)
  %17 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.24, i32 0, i32 0), i64 %elem25)
  ret i32 0

for.cond12:                                       ; preds = %for.inc14, %for.body7
  %j = load i64, i64* @j
  %for.cmp16 = icmp sle i64 %j, 2
  br i1 %for.cmp16, label %for.body13, label %for.end15

for.body13:                                       ; preds = %for.cond12
  %i17 = load i64, i64* @i
  %j18 = load i64, i64* @j
  %arrayidx19 = getelementptr inbounds [3 x [3 x i64]], [3 x [3 x i64]]* @m, i64 0, i64 %i17, i64 %j18
  %elem20 = load i64, i64* %arrayidx19
  %18 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.15, i32 0, i32 0), i64 %elem20)
  %19 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.17, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.16, i32 0, i32 0))
  br label %for.inc14

for.inc14:                                        ; preds = %for.body13
  %j21 = load i64, i64* @j
  %for.inc22 = add i64 %j21, 1
  store i64 %for.inc22, i64* @j
  br label %for.cond12

for.end15:                                        ; preds = %for.cond12
  %20 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.18, i32 0, i32 0))
  br label %for.inc8
}

declare i8* @memcpy(i8*, i8*, i64)
