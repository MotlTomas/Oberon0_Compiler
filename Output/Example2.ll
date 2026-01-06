; ModuleID = 'ControlFlow'
source_filename = "ControlFlow"
target triple = "x86_64-pc-windows-msvc"

@x = global i64 0
@grade = global i64 0
@isValid = global i1 false
@.str = private unnamed_addr constant [16 x i8] c"Greater than 50\00", align 1
@.fmt = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.1 = private unnamed_addr constant [12 x i8] c"Not greater\00", align 1
@.fmt.2 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.3 = private unnamed_addr constant [2 x i8] c"A\00", align 1
@.fmt.4 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.5 = private unnamed_addr constant [2 x i8] c"B\00", align 1
@.fmt.6 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.7 = private unnamed_addr constant [2 x i8] c"C\00", align 1
@.fmt.8 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.9 = private unnamed_addr constant [2 x i8] c"D\00", align 1
@.fmt.10 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.11 = private unnamed_addr constant [2 x i8] c"F\00", align 1
@.fmt.12 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.13 = private unnamed_addr constant [14 x i8] c"In range 1-99\00", align 1
@.fmt.14 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.15 = private unnamed_addr constant [14 x i8] c"Outside 10-50\00", align 1
@.fmt.16 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.17 = private unnamed_addr constant [9 x i8] c"Non-zero\00", align 1
@.fmt.18 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.19 = private unnamed_addr constant [18 x i8] c"Between 0 and 100\00", align 1
@.fmt.20 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define i32 @main() {
entry:
  store i64 75, i64* @x
  %x = load i64, i64* @x
  %cmp = icmp sgt i64 %x, 50
  br i1 %cmp, label %if.then, label %if.else

if.end:                                           ; preds = %if.else, %if.then
  store i64 85, i64* @grade
  %grade = load i64, i64* @grade
  %cmp4 = icmp sge i64 %grade, 90
  br i1 %cmp4, label %if.then2, label %elsif.cond.1

if.then:                                          ; preds = %entry
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([16 x i8], [16 x i8]* @.str, i32 0, i32 0))
  br label %if.end

if.else:                                          ; preds = %entry
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.2, i32 0, i32 0), i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.1, i32 0, i32 0))
  br label %if.end

if.end1:                                          ; preds = %if.else3, %elsif.then.3, %elsif.then.2, %elsif.then.1, %if.then2
  %x11 = load i64, i64* @x
  %cmp12 = icmp sgt i64 %x11, 0
  %x13 = load i64, i64* @x
  %cmp14 = icmp slt i64 %x13, 100
  %and = and i1 %cmp12, %cmp14
  store i1 %and, i1* @isValid
  %isValid = load i1, i1* @isValid
  br i1 %isValid, label %if.then16, label %if.end15

if.then2:                                         ; preds = %if.end
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.4, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.3, i32 0, i32 0))
  br label %if.end1

elsif.cond.1:                                     ; preds = %if.end
  %grade5 = load i64, i64* @grade
  %cmp6 = icmp sge i64 %grade5, 80
  br i1 %cmp6, label %elsif.then.1, label %elsif.cond.2

elsif.then.1:                                     ; preds = %elsif.cond.1
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.6, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.5, i32 0, i32 0))
  br label %if.end1

elsif.cond.2:                                     ; preds = %elsif.cond.1
  %grade7 = load i64, i64* @grade
  %cmp8 = icmp sge i64 %grade7, 70
  br i1 %cmp8, label %elsif.then.2, label %elsif.cond.3

elsif.then.2:                                     ; preds = %elsif.cond.2
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.8, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.7, i32 0, i32 0))
  br label %if.end1

elsif.cond.3:                                     ; preds = %elsif.cond.2
  %grade9 = load i64, i64* @grade
  %cmp10 = icmp sge i64 %grade9, 60
  br i1 %cmp10, label %elsif.then.3, label %if.else3

elsif.then.3:                                     ; preds = %elsif.cond.3
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.10, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.9, i32 0, i32 0))
  br label %if.end1

if.else3:                                         ; preds = %elsif.cond.3
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.12, i32 0, i32 0), i8* getelementptr inbounds ([2 x i8], [2 x i8]* @.str.11, i32 0, i32 0))
  br label %if.end1

if.end15:                                         ; preds = %if.then16, %if.end1
  %x17 = load i64, i64* @x
  %cmp18 = icmp slt i64 %x17, 10
  %x19 = load i64, i64* @x
  %cmp20 = icmp sgt i64 %x19, 50
  %or = or i1 %cmp18, %cmp20
  store i1 %or, i1* @isValid
  %isValid23 = load i1, i1* @isValid
  br i1 %isValid23, label %if.then22, label %if.end21

if.then16:                                        ; preds = %if.end1
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.14, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.13, i32 0, i32 0))
  br label %if.end15

if.end21:                                         ; preds = %if.then22, %if.end15
  %x26 = load i64, i64* @x
  %cmp27 = icmp eq i64 %x26, 0
  %not = xor i1 %cmp27, true
  br i1 %not, label %if.then25, label %if.end24

if.then22:                                        ; preds = %if.end15
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.16, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.15, i32 0, i32 0))
  br label %if.end21

if.end24:                                         ; preds = %if.then25, %if.end21
  %x30 = load i64, i64* @x
  %cmp31 = icmp sgt i64 %x30, 0
  br i1 %cmp31, label %if.then29, label %if.end28

if.then25:                                        ; preds = %if.end21
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.18, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.17, i32 0, i32 0))
  br label %if.end24

if.end28:                                         ; preds = %if.end32, %if.end24
  ret i32 0

if.then29:                                        ; preds = %if.end24
  %x34 = load i64, i64* @x
  %cmp35 = icmp slt i64 %x34, 100
  br i1 %cmp35, label %if.then33, label %if.end32

if.end32:                                         ; preds = %if.then33, %if.then29
  br label %if.end28

if.then33:                                        ; preds = %if.then29
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.20, i32 0, i32 0), i8* getelementptr inbounds ([18 x i8], [18 x i8]* @.str.19, i32 0, i32 0))
  br label %if.end32
}
