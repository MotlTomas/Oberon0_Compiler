; ModuleID = 'ForLoopTest'
source_filename = "ForLoopTest"
target triple = "x86_64-pc-windows-msvc"

@i = global i64 0
@sum = global i64 0
@five = global i64 0
@.fmt = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.1 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.2 = private unnamed_addr constant [13 x i8] c"Sum 1 to 5: \00", align 1
@.fmt.3 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.4 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1
@.str.5 = private unnamed_addr constant [11 x i8] c"Countdown:\00", align 1
@.fmt.6 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.fmt.7 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.8 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.9 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl.10 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.11 = private unnamed_addr constant [12 x i8] c"WHILE loop:\00", align 1
@.fmt.12 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.fmt.13 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.14 = private unnamed_addr constant [2 x i8] c" \00", align 1
@.fmt.15 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.nl.16 = private unnamed_addr constant [2 x i8] c"\0A\00", align 1
@.str.17 = private unnamed_addr constant [11 x i8] c"CASE test:\00", align 1
@.fmt.18 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.19 = private unnamed_addr constant [9 x i8] c"i is one\00", align 1
@.fmt.20 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.21 = private unnamed_addr constant [9 x i8] c"i is two\00", align 1
@.fmt.22 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.23 = private unnamed_addr constant [11 x i8] c"i is three\00", align 1
@.fmt.24 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.25 = private unnamed_addr constant [20 x i8] c"i is something else\00", align 1
@.fmt.26 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.27 = private unnamed_addr constant [6 x i8] c"Done!\00", align 1
@.fmt.28 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define i32 @main() {
entry:
  store i64 0, i64* @sum
  store i64 5, i64* @five
  store i64 1, i64* @i
  %five = load i64, i64* @five
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i = load i64, i64* @i
  %for.cmp = icmp sle i64 %i, %five
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %sum = load i64, i64* @sum
  %i1 = load i64, i64* @i
  %add = add i64 %sum, %i1
  store i64 %add, i64* @sum
  %i2 = load i64, i64* @i
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt, i32 0, i32 0), i64 %i2)
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.1, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str, i32 0, i32 0))
  br label %for.inc

for.inc:                                          ; preds = %for.body
  %i3 = load i64, i64* @i
  %for.inc4 = add i64 %i3, 1
  store i64 %for.inc4, i64* @i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl, i32 0, i32 0))
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.3, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.2, i32 0, i32 0))
  %sum5 = load i64, i64* @sum
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.4, i32 0, i32 0), i64 %sum5)
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.6, i32 0, i32 0), i8* getelementptr inbounds ([11 x i8], [11 x i8]* @.str.5, i32 0, i32 0))
  store i64 5, i64* @i
  br label %for.cond6

for.cond6:                                        ; preds = %for.inc8, %for.end
  %i10 = load i64, i64* @i
  %for.cmp11 = icmp sge i64 %i10, 1
  br i1 %for.cmp11, label %for.body7, label %for.end9

for.body7:                                        ; preds = %for.cond6
  %i12 = load i64, i64* @i
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.7, i32 0, i32 0), i64 %i12)
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.9, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.8, i32 0, i32 0))
  br label %for.inc8

for.inc8:                                         ; preds = %for.body7
  %i13 = load i64, i64* @i
  %for.dec = sub i64 %i13, 1
  store i64 %for.dec, i64* @i
  br label %for.cond6

for.end9:                                         ; preds = %for.cond6
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.10, i32 0, i32 0))
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.12, i32 0, i32 0), i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.11, i32 0, i32 0))
  store i64 1, i64* @i
  br label %while.cond

while.cond:                                       ; preds = %while.body, %for.end9
  %i14 = load i64, i64* @i
  %cmp = icmp sle i64 %i14, 3
  br i1 %cmp, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %i15 = load i64, i64* @i
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.13, i32 0, i32 0), i64 %i15)
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.15, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.14, i32 0, i32 0))
  %i16 = load i64, i64* @i
  %add17 = add i64 %i16, 1
  store i64 %add17, i64* @i
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.nl.16, i32 0, i32 0))
  %13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.18, i32 0, i32 0), i8* getelementptr inbounds ([11 x i8], [11 x i8]* @.str.17, i32 0, i32 0))
  store i64 2, i64* @i
  %i18 = load i64, i64* @i
  br label %case.cond.0

case.cond.0:                                      ; preds = %while.end
  %case.cmp.0 = icmp eq i64 %i18, 1
  br i1 %case.cmp.0, label %case.body.0, label %case.cond.1

case.body.0:                                      ; preds = %case.cond.0
  %14 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.20, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.19, i32 0, i32 0))
  br label %case.end

case.cond.1:                                      ; preds = %case.cond.0
  %case.cmp.1 = icmp eq i64 %i18, 2
  br i1 %case.cmp.1, label %case.body.1, label %case.cond.2

case.body.1:                                      ; preds = %case.cond.1
  %15 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.22, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.21, i32 0, i32 0))
  br label %case.end

case.cond.2:                                      ; preds = %case.cond.1
  %case.cmp.2 = icmp eq i64 %i18, 3
  br i1 %case.cmp.2, label %case.body.2, label %case.else

case.body.2:                                      ; preds = %case.cond.2
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.24, i32 0, i32 0), i8* getelementptr inbounds ([11 x i8], [11 x i8]* @.str.23, i32 0, i32 0))
  br label %case.end

case.else:                                        ; preds = %case.cond.2
  %17 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.26, i32 0, i32 0), i8* getelementptr inbounds ([20 x i8], [20 x i8]* @.str.25, i32 0, i32 0))
  br label %case.end

case.end:                                         ; preds = %case.else, %case.body.2, %case.body.1, %case.body.0
  %18 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.28, i32 0, i32 0), i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.str.27, i32 0, i32 0))
  ret i32 0
}
