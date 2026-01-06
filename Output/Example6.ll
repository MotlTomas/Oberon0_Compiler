; ModuleID = 'CaseSwitch'
source_filename = "CaseSwitch"
target triple = "x86_64-pc-windows-msvc"

@day = global i64 0
@month = global i64 0
@code = global i64 0
@.str = private unnamed_addr constant [11 x i8] c"Day 3 is: \00", align 1
@.fmt = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.str.1 = private unnamed_addr constant [7 x i8] c"Monday\00", align 1
@.fmt.2 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.3 = private unnamed_addr constant [8 x i8] c"Tuesday\00", align 1
@.fmt.4 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.5 = private unnamed_addr constant [10 x i8] c"Wednesday\00", align 1
@.fmt.6 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.7 = private unnamed_addr constant [9 x i8] c"Thursday\00", align 1
@.fmt.8 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.9 = private unnamed_addr constant [7 x i8] c"Friday\00", align 1
@.fmt.10 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.11 = private unnamed_addr constant [9 x i8] c"Saturday\00", align 1
@.fmt.12 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.13 = private unnamed_addr constant [7 x i8] c"Sunday\00", align 1
@.fmt.14 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.15 = private unnamed_addr constant [12 x i8] c"Invalid day\00", align 1
@.fmt.16 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.17 = private unnamed_addr constant [14 x i8] c"Month 7 has: \00", align 1
@.fmt.18 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.str.19 = private unnamed_addr constant [8 x i8] c"31 days\00", align 1
@.fmt.20 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.21 = private unnamed_addr constant [8 x i8] c"30 days\00", align 1
@.fmt.22 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.23 = private unnamed_addr constant [14 x i8] c"28 or 29 days\00", align 1
@.fmt.24 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.25 = private unnamed_addr constant [14 x i8] c"Invalid month\00", align 1
@.fmt.26 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.27 = private unnamed_addr constant [10 x i8] c"Code 99: \00", align 1
@.fmt.28 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.str.29 = private unnamed_addr constant [5 x i8] c"Zero\00", align 1
@.fmt.30 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.31 = private unnamed_addr constant [4 x i8] c"One\00", align 1
@.fmt.32 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.33 = private unnamed_addr constant [4 x i8] c"Two\00", align 1
@.fmt.34 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.35 = private unnamed_addr constant [13 x i8] c"Unknown code\00", align 1
@.fmt.36 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.37 = private unnamed_addr constant [12 x i8] c"Categories:\00", align 1
@.fmt.38 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.fmt.39 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1
@.str.40 = private unnamed_addr constant [5 x i8] c" -> \00", align 1
@.fmt.41 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.str.42 = private unnamed_addr constant [4 x i8] c"Low\00", align 1
@.fmt.43 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.44 = private unnamed_addr constant [7 x i8] c"Medium\00", align 1
@.fmt.45 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.str.46 = private unnamed_addr constant [5 x i8] c"High\00", align 1
@.fmt.47 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define i32 @main() {
entry:
  store i64 3, i64* @day
  %0 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([11 x i8], [11 x i8]* @.str, i32 0, i32 0))
  %day = load i64, i64* @day
  br label %case.cond.0

case.cond.0:                                      ; preds = %entry
  %case.cmp.0 = icmp eq i64 %day, 1
  br i1 %case.cmp.0, label %case.body.0, label %case.cond.1

case.body.0:                                      ; preds = %case.cond.0
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.2, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.1, i32 0, i32 0))
  br label %case.end

case.cond.1:                                      ; preds = %case.cond.0
  %case.cmp.1 = icmp eq i64 %day, 2
  br i1 %case.cmp.1, label %case.body.1, label %case.cond.2

case.body.1:                                      ; preds = %case.cond.1
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.4, i32 0, i32 0), i8* getelementptr inbounds ([8 x i8], [8 x i8]* @.str.3, i32 0, i32 0))
  br label %case.end

case.cond.2:                                      ; preds = %case.cond.1
  %case.cmp.2 = icmp eq i64 %day, 3
  br i1 %case.cmp.2, label %case.body.2, label %case.cond.3

case.body.2:                                      ; preds = %case.cond.2
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.6, i32 0, i32 0), i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.5, i32 0, i32 0))
  br label %case.end

case.cond.3:                                      ; preds = %case.cond.2
  %case.cmp.3 = icmp eq i64 %day, 4
  br i1 %case.cmp.3, label %case.body.3, label %case.cond.4

case.body.3:                                      ; preds = %case.cond.3
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.8, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.7, i32 0, i32 0))
  br label %case.end

case.cond.4:                                      ; preds = %case.cond.3
  %case.cmp.4 = icmp eq i64 %day, 5
  br i1 %case.cmp.4, label %case.body.4, label %case.cond.5

case.body.4:                                      ; preds = %case.cond.4
  %5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.10, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.9, i32 0, i32 0))
  br label %case.end

case.cond.5:                                      ; preds = %case.cond.4
  %case.cmp.5 = icmp eq i64 %day, 6
  br i1 %case.cmp.5, label %case.body.5, label %case.cond.6

case.body.5:                                      ; preds = %case.cond.5
  %6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.12, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.11, i32 0, i32 0))
  br label %case.end

case.cond.6:                                      ; preds = %case.cond.5
  %case.cmp.6 = icmp eq i64 %day, 7
  br i1 %case.cmp.6, label %case.body.6, label %case.else

case.body.6:                                      ; preds = %case.cond.6
  %7 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.14, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.13, i32 0, i32 0))
  br label %case.end

case.else:                                        ; preds = %case.cond.6
  %8 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.16, i32 0, i32 0), i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.15, i32 0, i32 0))
  br label %case.end

case.end:                                         ; preds = %case.else, %case.body.6, %case.body.5, %case.body.4, %case.body.3, %case.body.2, %case.body.1, %case.body.0
  store i64 7, i64* @month
  %9 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.18, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.17, i32 0, i32 0))
  %month = load i64, i64* @month
  br label %case.cond.01

case.cond.01:                                     ; preds = %case.end
  %case.cmp.09 = icmp eq i64 %month, 1
  %case.cmp.010 = icmp eq i64 %month, 3
  %case.or = or i1 %case.cmp.09, %case.cmp.010
  %case.cmp.011 = icmp eq i64 %month, 5
  %case.or12 = or i1 %case.or, %case.cmp.011
  %case.cmp.013 = icmp eq i64 %month, 7
  %case.or14 = or i1 %case.or12, %case.cmp.013
  %case.cmp.015 = icmp eq i64 %month, 8
  %case.or16 = or i1 %case.or14, %case.cmp.015
  %case.cmp.017 = icmp eq i64 %month, 10
  %case.or18 = or i1 %case.or16, %case.cmp.017
  %case.cmp.019 = icmp eq i64 %month, 12
  %case.or20 = or i1 %case.or18, %case.cmp.019
  br i1 %case.or20, label %case.body.02, label %case.cond.13

case.body.02:                                     ; preds = %case.cond.01
  %10 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.20, i32 0, i32 0), i8* getelementptr inbounds ([8 x i8], [8 x i8]* @.str.19, i32 0, i32 0))
  br label %case.end8

case.cond.13:                                     ; preds = %case.cond.01
  %case.cmp.121 = icmp eq i64 %month, 4
  %case.cmp.122 = icmp eq i64 %month, 6
  %case.or23 = or i1 %case.cmp.121, %case.cmp.122
  %case.cmp.124 = icmp eq i64 %month, 9
  %case.or25 = or i1 %case.or23, %case.cmp.124
  %case.cmp.126 = icmp eq i64 %month, 11
  %case.or27 = or i1 %case.or25, %case.cmp.126
  br i1 %case.or27, label %case.body.14, label %case.cond.25

case.body.14:                                     ; preds = %case.cond.13
  %11 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.22, i32 0, i32 0), i8* getelementptr inbounds ([8 x i8], [8 x i8]* @.str.21, i32 0, i32 0))
  br label %case.end8

case.cond.25:                                     ; preds = %case.cond.13
  %case.cmp.228 = icmp eq i64 %month, 2
  br i1 %case.cmp.228, label %case.body.26, label %case.else7

case.body.26:                                     ; preds = %case.cond.25
  %12 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.24, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.23, i32 0, i32 0))
  br label %case.end8

case.else7:                                       ; preds = %case.cond.25
  %13 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.26, i32 0, i32 0), i8* getelementptr inbounds ([14 x i8], [14 x i8]* @.str.25, i32 0, i32 0))
  br label %case.end8

case.end8:                                        ; preds = %case.else7, %case.body.26, %case.body.14, %case.body.02
  store i64 99, i64* @code
  %14 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.28, i32 0, i32 0), i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.27, i32 0, i32 0))
  %code = load i64, i64* @code
  br label %case.cond.029

case.cond.029:                                    ; preds = %case.end8
  %case.cmp.037 = icmp eq i64 %code, 0
  br i1 %case.cmp.037, label %case.body.030, label %case.cond.131

case.body.030:                                    ; preds = %case.cond.029
  %15 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.30, i32 0, i32 0), i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.str.29, i32 0, i32 0))
  br label %case.end36

case.cond.131:                                    ; preds = %case.cond.029
  %case.cmp.138 = icmp eq i64 %code, 1
  br i1 %case.cmp.138, label %case.body.132, label %case.cond.233

case.body.132:                                    ; preds = %case.cond.131
  %16 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.32, i32 0, i32 0), i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.str.31, i32 0, i32 0))
  br label %case.end36

case.cond.233:                                    ; preds = %case.cond.131
  %case.cmp.239 = icmp eq i64 %code, 2
  br i1 %case.cmp.239, label %case.body.234, label %case.else35

case.body.234:                                    ; preds = %case.cond.233
  %17 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.34, i32 0, i32 0), i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.str.33, i32 0, i32 0))
  br label %case.end36

case.else35:                                      ; preds = %case.cond.233
  %18 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.36, i32 0, i32 0), i8* getelementptr inbounds ([13 x i8], [13 x i8]* @.str.35, i32 0, i32 0))
  br label %case.end36

case.end36:                                       ; preds = %case.else35, %case.body.234, %case.body.132, %case.body.030
  %19 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.38, i32 0, i32 0), i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.37, i32 0, i32 0))
  store i64 0, i64* @code
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %case.end36
  %code40 = load i64, i64* @code
  %for.cmp = icmp sle i64 %code40, 5
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %code41 = load i64, i64* @code
  %20 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.39, i32 0, i32 0), i64 %code41)
  %21 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.41, i32 0, i32 0), i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.str.40, i32 0, i32 0))
  %code42 = load i64, i64* @code
  br label %case.cond.043

for.inc:                                          ; preds = %case.end50
  %code60 = load i64, i64* @code
  %for.inc61 = add i64 %code60, 1
  store i64 %for.inc61, i64* @code
  br label %for.cond

for.end:                                          ; preds = %for.cond
  ret i32 0

case.cond.043:                                    ; preds = %for.body
  %case.cmp.051 = icmp eq i64 %code42, 0
  %case.cmp.052 = icmp eq i64 %code42, 1
  %case.or53 = or i1 %case.cmp.051, %case.cmp.052
  br i1 %case.or53, label %case.body.044, label %case.cond.145

case.body.044:                                    ; preds = %case.cond.043
  %22 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.43, i32 0, i32 0), i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.str.42, i32 0, i32 0))
  br label %case.end50

case.cond.145:                                    ; preds = %case.cond.043
  %case.cmp.154 = icmp eq i64 %code42, 2
  %case.cmp.155 = icmp eq i64 %code42, 3
  %case.or56 = or i1 %case.cmp.154, %case.cmp.155
  br i1 %case.or56, label %case.body.146, label %case.cond.247

case.body.146:                                    ; preds = %case.cond.145
  %23 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.45, i32 0, i32 0), i8* getelementptr inbounds ([7 x i8], [7 x i8]* @.str.44, i32 0, i32 0))
  br label %case.end50

case.cond.247:                                    ; preds = %case.cond.145
  %case.cmp.257 = icmp eq i64 %code42, 4
  %case.cmp.258 = icmp eq i64 %code42, 5
  %case.or59 = or i1 %case.cmp.257, %case.cmp.258
  br i1 %case.or59, label %case.body.248, label %case.else49

case.body.248:                                    ; preds = %case.cond.247
  %24 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt.47, i32 0, i32 0), i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.str.46, i32 0, i32 0))
  br label %case.end50

case.else49:                                      ; preds = %case.cond.247
  br label %case.end50

case.end50:                                       ; preds = %case.else49, %case.body.248, %case.body.146, %case.body.044
  br label %for.inc
}
