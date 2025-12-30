; ModuleID = 'ControlDemo'
source_filename = "ControlDemo"
target triple = "x86_64-pc-linux-gnu"

@n = global i64 0
@result = global i64 0
@.fmt = private unnamed_addr constant [5 x i8] c"%lld\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define i64 @Outer(i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %acc = alloca i64
  store i64 0, i64* %acc
  %acc1 = load i64, i64* %acc
  ret i64 %acc1
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
  %result = load i64, i64* @result
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt, i32 0, i32 0), i64 %result)
  ret i32 0
}
