; ModuleID = 'Loops'
source_filename = "Loops"
target triple = "x86_64-pc-windows-msvc"

@i = global i64 0
@sum = global i64 0
@.str = private unnamed_addr constant [16 x i8] c"WHILE sum 1-5: \00", align 1
@.fmt = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.1 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.2 = private unnamed_addr constant [14 x i8] c"FOR sum 1-5: \00", align 1
@.fmt.3 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.4 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.5 = private unnamed_addr constant [12 x i8] c"Countdown: \00", align 1
@.fmt.6 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.7 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.8 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.9 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.10 = private unnamed_addr constant [13 x i8] c"REPEAT sum: \00", align 1
@.fmt.11 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.12 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.13 = private unnamed_addr constant [13 x i8] c"BREAK at 5: \00", align 1
@.fmt.14 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.15 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.16 = private unnamed_addr constant [15 x i8] c"Odd sum 1-10: \00", align 1
@.fmt.17 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.18 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.fmt.19 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.20 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.21 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl.22 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define i32 @main() {
entry:
  store i64 0, i64* @sum
  store i64 1, i64* @i
  br label %while.cond

while.cond:                                       ; preds = %while.body, %entry
  %i = load i64, i64* @i
  %cmp = icmp sle i64 %i, 5
  br i1 %cmp, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %sum = load i64, i64* @sum
  %i1 = load i64, i64* @i
  %add = add i64 %sum, %i1
  store i64 %add, i64* @sum
  %i2 = load i64, i64* @i
  %add3 = add i64 %i2, 1
  store i64 %add3, i64* @i
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([16 x i8], [16 x i8]* @.str, i32 0, i32 0))
  %sum4 = load i64, i64* @sum
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.1, i32 0, i32 0), i64 %sum4)
  store i64 0, i64* @sum
  store i64 1, i64* @i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %while.end
  %i5 = load i64, i64* @i
  %for.cmp = icmp sle i64 %i5, 5
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %sum6 = load i64, i64* @sum
  %i7 = load i64, i64* @i
  %add8 = add i64 %sum6, %i7
  store i64 %add8, i64* @sum
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %i9 = load i64, i64* @i
  %for.inc10 = add i64 %i9, 1
  store i64 %for.inc10, i64* @i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.3, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.2, i32 0, i32 0))
  %sum11 = load i64, i64* @sum
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.4, i32 0, i32 0), i64 %sum11)
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.6, i32 0, i32 0), i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.5, i32 0, i32 0))
  store i64 5, i64* @i
  br label %for.cond12

for.cond12:                                       ; preds = %for.inc14, %for.end
  %i16 = load i64, i64* @i
  %for.cmp17 = icmp sge i64 %i16, 1
  br i1 %for.cmp17, label %for.body13, label %for.end15

for.body13:                                       ; preds = %for.cond12
  %i18 = load i64, i64* @i
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.7, i32 0, i32 0), i64 %i18)
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.9, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.8, i32 0, i32 0))
  br label %for.inc14

for.inc14:                                        ; preds = %for.body13
  %i19 = load i64, i64* @i
  %for.dec = sub i64 %i19, 1
  store i64 %for.dec, i64* @i
  br label %for.cond12

for.end15:                                        ; preds = %for.cond12
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  store i64 0, i64* @sum
  store i64 1, i64* @i
  br label %repeat.body

repeat.body:                                      ; preds = %repeat.cond, %for.end15
  %sum20 = load i64, i64* @sum
  %i21 = load i64, i64* @i
  %add22 = add i64 %sum20, %i21
  store i64 %add22, i64* @sum
  %i23 = load i64, i64* @i
  %add24 = add i64 %i23, 1
  store i64 %add24, i64* @i
  br label %repeat.cond

repeat.cond:                                      ; preds = %repeat.body
  %i25 = load i64, i64* @i
  %cmp26 = icmp sgt i64 %i25, 5
  br i1 %cmp26, label %repeat.end, label %repeat.body

repeat.end:                                       ; preds = %repeat.cond
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.11, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.10, i32 0, i32 0))
  %sum27 = load i64, i64* @sum
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.12, i32 0, i32 0), i64 %sum27)
  store i64 0, i64* @sum
  store i64 1, i64* @i
  br label %for.cond28

for.cond28:                                       ; preds = %for.inc30, %repeat.end
  %i32 = load i64, i64* @i
  %for.cmp33 = icmp sle i64 %i32, 100
  br i1 %for.cmp33, label %for.body29, label %for.end31

for.body29:                                       ; preds = %for.cond28
  %i34 = load i64, i64* @i
  %cmp35 = icmp sgt i64 %i34, 5
  br i1 %cmp35, label %if.then, label %if.end

for.inc30:                                        ; preds = %if.end
  %i39 = load i64, i64* @i
  %for.inc40 = add i64 %i39, 1
  store i64 %for.inc40, i64* @i
  br label %for.cond28

for.end31:                                        ; preds = %if.then, %for.cond28
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.14, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.13, i32 0, i32 0))
  %sum41 = load i64, i64* @sum
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.15, i32 0, i32 0), i64 %sum41)
  store i64 0, i64* @sum
  store i64 1, i64* @i
  br label %for.cond42

if.end:                                           ; preds = %break.unreachable, %for.body29
  %sum36 = load i64, i64* @sum
  %i37 = load i64, i64* @i
  %add38 = add i64 %sum36, %i37
  store i64 %add38, i64* @sum
  br label %for.inc30

if.then:                                          ; preds = %for.body29
  br label %for.end31

break.unreachable:                                ; No predecessors!
  br label %if.end

for.cond42:                                       ; preds = %for.inc44, %for.end31
  %i46 = load i64, i64* @i
  %for.cmp47 = icmp sle i64 %i46, 10
  br i1 %for.cmp47, label %for.body43, label %for.end45

for.body43:                                       ; preds = %for.cond42
  %i50 = load i64, i64* @i
  %rem = srem i64 %i50, 2
  %cmp51 = icmp eq i64 %rem, 0
  br i1 %cmp51, label %if.then49, label %if.end48

for.inc44:                                        ; preds = %if.end48, %if.then49
  %i55 = load i64, i64* @i
  %for.inc56 = add i64 %i55, 1
  store i64 %for.inc56, i64* @i
  br label %for.cond42

for.end45:                                        ; preds = %for.cond42
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.17, i32 0, i32 0), i8* getelementptr inbounds ([15 x i8], [15 x i8]* @.str.16, i32 0, i32 0))
  %sum57 = load i64, i64* @sum
  %13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.18, i32 0, i32 0), i64 %sum57)
  store i64 1, i64* @i
  br label %for.cond58

if.end48:                                         ; preds = %continue.unreachable, %for.body43
  %sum52 = load i64, i64* @sum
  %i53 = load i64, i64* @i
  %add54 = add i64 %sum52, %i53
  store i64 %add54, i64* @sum
  br label %for.inc44

if.then49:                                        ; preds = %for.body43
  br label %for.inc44

continue.unreachable:                             ; No predecessors!
  br label %if.end48

for.cond58:                                       ; preds = %for.inc60, %for.end45
  %i62 = load i64, i64* @i
  %for.cmp63 = icmp sle i64 %i62, 3
  br i1 %for.cmp63, label %for.body59, label %for.end61

for.body59:                                       ; preds = %for.cond58
  store i64 1, i64* @sum
  br label %for.cond64

for.inc60:                                        ; preds = %for.end67
  %i74 = load i64, i64* @i
  %for.inc75 = add i64 %i74, 1
  store i64 %for.inc75, i64* @i
  br label %for.cond58

for.end61:                                        ; preds = %for.cond58
  ret i32 0

for.cond64:                                       ; preds = %for.inc66, %for.body59
  %sum68 = load i64, i64* @sum
  %for.cmp69 = icmp sle i64 %sum68, 3
  br i1 %for.cmp69, label %for.body65, label %for.end67

for.body65:                                       ; preds = %for.cond64
  %i70 = load i64, i64* @i
  %sum71 = load i64, i64* @sum
  %mul = mul i64 %i70, %sum71
  %14 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.19, i32 0, i32 0), i64 %mul)
  %15 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.21, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.20, i32 0, i32 0))
  br label %for.inc66

for.inc66:                                        ; preds = %for.body65
  %sum72 = load i64, i64* @sum
  %for.inc73 = add i64 %sum72, 1
  store i64 %for.inc73, i64* @sum
  br label %for.cond64

for.end67:                                        ; preds = %for.cond64
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.22, i32 0, i32 0))
  br label %for.inc60
}
