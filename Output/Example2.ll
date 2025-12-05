; Module: ArrayDemo
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

@mat = global [2 x [2 x i64]] zeroinitializer
@res = global [2 x [2 x i64]] zeroinitializer

define void @FillMatrix([2 x [2 x i64]]* %a) {
  entry:
  %i = alloca i64
  %j = alloca i64
  store i64 0, i64* %i
  br label %for.cond0
for.cond0:
  %t0 = load i64, i64* %i
  %t1 = icmp sle i64 %t0, 1
  br i1 %t1, label %for.body1, label %for.end3
for.body1:
  store i64 0, i64* %j
  br label %for.cond4
for.cond4:
  %t2 = load i64, i64* %j
  %t3 = icmp sle i64 %t2, 1
  br i1 %t3, label %for.body5, label %for.end7
for.body5:
  %t4 = load i64, i64* %i
  %t5 = mul i64 %t4, 10
  %t6 = load i64, i64* %j
  %t7 = add i64 %t5, %t6
  %t9 = load i64, i64* %i
  %t10 = load i64, i64* %j
  %t11 = getelementptr [2 x [2 x i64]], [2 x [2 x i64]]* %a, i64 0, i64 %t9, i64 %t10
  store [2 x [2 x i64]] %t7, [2 x [2 x i64]]* %t11
  ret void
}

define void @SumMatrix([2 x [2 x i64]] %a) {
  entry:
  %a.addr = alloca [2 x [2 x i64]]
  store [2 x [2 x i64]] %a, [2 x [2 x i64]]* %a.addr
  %i = alloca i64
  %j = alloca i64
  %sum = alloca i64
  store i64 0, i64* %sum
  store i64 0, i64* %i
  br label %for.cond8
for.cond8:
  %t12 = load i64, i64* %i
  %t13 = icmp sle i64 %t12, 1
  br i1 %t13, label %for.body9, label %for.end11
for.body9:
  store i64 0, i64* %j
  br label %for.cond12
for.cond12:
  %t14 = load i64, i64* %j
  %t15 = icmp sle i64 %t14, 1
  br i1 %t15, label %for.body13, label %for.end15
for.body13:
  %t16 = load i64, i64* %sum
  %t17 = load i64, i64* %i
  %t18 = load i64, i64* %j
  %t19 = getelementptr [2 x [2 x i64]], [2 x [2 x i64]]* %a.addr, i64 0, i64 %t17, i64 %t18
  %t20 = load [2 x [2 x i64]], [2 x [2 x i64]]* %t19
  %t22 = add i64 %t16, %t20
  store i64 %t22, i64* %sum
  %t23 = load i64, i64* %sum
  ret void %t23
  ret void
}
  %t25 = load [2 x [2 x i64]], [2 x [2 x i64]]* @mat
  call void @FillMatrix([2 x [2 x i64]]* %t25)
  %t26 = load [2 x [2 x i64]], [2 x [2 x i64]]* @mat
  store [2 x [2 x i64]] %t26, [2 x [2 x i64]]* @res

; Main entry point
define i32 @main() {
entry:
  ret i32 0
}
