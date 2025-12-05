; Module: ControlDemo
target triple = "x86_64-pc-linux-gnu"

declare i32 @printf(i8*, ...)
declare i32 @scanf(i8*, ...)
declare i32 @puts(i8*)

@.int_fmt = private unnamed_addr constant [5 x i8] c"%lld\00"
@.real_fmt = private unnamed_addr constant [4 x i8] c"%lf\00"
@.str_fmt = private unnamed_addr constant [3 x i8] c"%s\00"
@.newline = private unnamed_addr constant [2 x i8] c"\0A\00"
@.scan_int = private unnamed_addr constant [5 x i8] c"%lld\00"
@.scan_real = private unnamed_addr constant [4 x i8] c"%lf\00"

@n = global i64 zeroinitializer
@result = global i64 zeroinitializer

define void @Outer(i64 %x) {
  entry:
  %x.addr = alloca i64
  store i64 %x, i64* %x.addr
  %acc = alloca i64

define void @Inner(i64 %y) {
  entry:
  %y.addr = alloca i64
  store i64 %y, i64* %y.addr
  %t0 = load i64, i64* %y.addr
  %t1 = mul i64 %t0, 2
  ret void %t1
  ret void
}
  store i64 0, i64* %acc
  br label %while.cond0
while.cond0:
  %t3 = load i64, i64* %x.addr
  %t4 = icmp sgt i64 %t3, 0
  br i1 %t4, label %while.body1, label %while.end2
while.body1:
  %t5 = load i64, i64* %acc
  %t6 = load i64, i64* %x.addr
  %t7 = call void @Inner(i64 %t6)
  %t9 = add i64 %t5, %t7
  store i64 %t9, i64* %acc
  %t10 = load i64, i64* %x.addr
  %t11 = sub i64 %t10, 1
  store i64 %t11, i64* %x.addr
  br label %while.cond0
while.end2:
  %t12 = load i64, i64* %acc
  ret void %t12
  ret void
}
  store i64 5, i64* @n
  %t14 = load i64, i64* @n
  %t15 = icmp sge i64 %t14, 5
  br i1 %t15, label %if.then3, label %if.else4
if.then3:
  %t16 = load i64, i64* @n
  %t17 = call void @Outer(i64 %t16)
  store i64 %t17, i64* @result
  store i64 0, i64* @result
  br label %if.end6
if.else7:
  br label %if.end6
if.end6:

; Main entry point
define i32 @main() {
entry:
  ret i32 0
}
