; Module: TypesDemo
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

@i = global i64 zeroinitializer
@r = global double zeroinitializer
@b = global i1 zeroinitializer
@s = global i8* zeroinitializer

define void @IntToReal(i64 %x) {
  entry:
  %x.addr = alloca i64
  store i64 %x, i64* %x.addr
  %t0 = load i64, i64* %x.addr
  ret void %t0
  ret void
}

define void @RealToInt(double %x) {
  entry:
  %x.addr = alloca double
  store double %x, double* %x.addr
  %t2 = load double, double* %x.addr
  ret void %t2
  ret void
}

define void @BoolToString(i1 %val) {
  entry:
  %val.addr = alloca i1
  store i1 %val, i1* %val.addr
  %t4 = load i1, i1* %val.addr
  br i1 %t4, label %if.then0, label %if.else1
if.then0:
@.str0 = private unnamed_addr constant [5 x i8] c"true\00"
  %t5 = getelementptr [5 x i8], [5 x i8]* @.str0, i32 0, i32 0
  ret void %t5
@.str1 = private unnamed_addr constant [6 x i8] c"false\00"
  %t7 = getelementptr [6 x i8], [6 x i8]* @.str1, i32 0, i32 0
  ret void %t7
  br label %if.end3
if.else4:
  br label %if.end3
if.end3:
  ret void
}
  store i64 5, i64* @i
  store double 6.7, double* @r
  %t9 = load i64, i64* @i
  %t10 = icmp slt i64 %t9, 10
  store i1 %t10, i1* @b
@.str2 = private unnamed_addr constant [9 x i8] c"Result: \00"
  %t11 = getelementptr [9 x i8], [9 x i8]* @.str2, i32 0, i32 0
  store i8* %t11, i8** @s
  %t12 = load i64, i64* @i
  %t13 = call void @IntToReal(i64 %t12)
  store double %t13, double* @r
  %t15 = load double, double* @r
  %t16 = call void @RealToInt(double %t15)
  store i64 %t16, i64* @i
  %t18 = load i1, i1* @b
  %t19 = call void @BoolToString(i1 %t18)
  store i8* %t19, i8** @s

; Main entry point
define i32 @main() {
entry:
  ret i32 0
}
