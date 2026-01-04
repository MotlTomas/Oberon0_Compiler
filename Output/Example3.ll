; ModuleID = 'ControlDemo'
source_filename = "ControlDemo"
target triple = "x86_64-pc-windows-msvc"

@n = global i64 0
@result = global i64 0
@.str = private unnamed_addr constant [9 x i8] c"n is one\00", align 1
@.fmt = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.str.1 = private unnamed_addr constant [10 x i8] c"n is five\00", align 1
@.fmt.2 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.str.3 = private unnamed_addr constant [12 x i8] c"other value\00", align 1
@.fmt.4 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@.fmt.5 = private unnamed_addr constant [5 x i8] c"%lld\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define i64 @Outer(i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %acc = alloca i64
  store i64 0, i64* %acc
  br label %while.cond

while.cond:                                       ; preds = %while.body, %entry
  %x = load i64, i64* %x.addr
  %cmp = icmp sgt i64 %x, 0
  br i1 %cmp, label %while.body, label %while.end

while.body:                                       ; preds = %while.cond
  %acc1 = load i64, i64* %acc
  %x2 = load i64, i64* %x.addr
  %1 = call i64 @Inner(i64 %x2)
  %add = add i64 %acc1, %1
  store i64 %add, i64* %acc
  %x3 = load i64, i64* %x.addr
  %sub = sub i64 %x3, 1
  store i64 %sub, i64* %x.addr
  br label %while.cond

while.end:                                        ; preds = %while.cond
  %acc4 = load i64, i64* %acc
  ret i64 %acc4
}

define i64 @Inner(i64) {
entry:
  %y.addr = alloca i64
  store i64 %0, i64* %y.addr
  %y = load i64, i64* %y.addr
  %mul = mul i64 %y, 2
  ret i64 %mul
}

define i32 @main() {
entry:
  store i64 5, i64* @n
  %n = load i64, i64* @n
  %cmp = icmp sge i64 %n, 5
  br i1 %cmp, label %if.then, label %if.else

if.then:                                          ; preds = %entry
  %n1 = load i64, i64* @n
  %0 = call i64 @Outer(i64 %n1)
  store i64 %0, i64* @result
  br label %if.end

if.else:                                          ; preds = %entry
  store i64 0, i64* @result
  br label %if.end

if.end:                                           ; preds = %if.else, %if.then
  %n2 = load i64, i64* @n
  br label %case.cond.0

case.cond.0:                                      ; preds = %if.end
  %case.cmp.0 = icmp eq i64 %n2, 1
  br i1 %case.cmp.0, label %case.body.0, label %case.cond.1

case.body.0:                                      ; preds = %case.cond.0
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str, i32 0, i32 0))
  br label %case.end

case.cond.1:                                      ; preds = %case.cond.0
  %case.cmp.1 = icmp eq i64 %n2, 5
  br i1 %case.cmp.1, label %case.body.1, label %case.else

case.body.1:                                      ; preds = %case.cond.1
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.2, i32 0, i32 0), i8* getelementptr inbounds ([10 x i8], [10 x i8]* @.str.1, i32 0, i32 0))
  br label %case.end

case.else:                                        ; preds = %case.cond.1
  %3 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @.fmt.4, i32 0, i32 0), i8* getelementptr inbounds ([12 x i8], [12 x i8]* @.str.3, i32 0, i32 0))
  br label %case.end

case.end:                                         ; preds = %case.else, %case.body.1, %case.body.0
  %result = load i64, i64* @result
  %4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt.5, i32 0, i32 0), i64 %result)
  ret i32 0
}
