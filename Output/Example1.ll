; ModuleID = 'TypesDemo'
source_filename = "TypesDemo"
target triple = "x86_64-pc-linux-gnu"

@i = global i64 0
@r = global double 0.000000e+00
@b = global i1 false
@s = global i8* null
@.str = private unnamed_addr constant [5 x i8] c"true\00", align 1
@.str.1 = private unnamed_addr constant [6 x i8] c"false\00", align 1
@.str.2 = private unnamed_addr constant [9 x i8] c"Result: \00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define double @IntToReal(i64) {
entry:
  %x.addr = alloca i64
  store i64 %0, i64* %x.addr
  %x = load i64, i64* %x.addr
  %sitofp = sitofp i64 %x to double
  ret double %sitofp
}

define i64 @RealToInt(double) {
entry:
  %x.addr = alloca double
  store double %0, double* %x.addr
  %x = load double, double* %x.addr
  %fptosi = fptosi double %x to i64
  ret i64 %fptosi
}

define i8* @BoolToString(i1) {
entry:
  %val.addr = alloca i1
  store i1 %0, i1* %val.addr
  %val = load i1, i1* %val.addr
  br i1 %val, label %if.then, label %if.else

if.then:                                          ; preds = %entry
  ret i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.str, i32 0, i32 0)

if.else:                                          ; preds = %entry
  ret i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.str.1, i32 0, i32 0)

if.end:                                           ; No predecessors!
  ret i8* null
}

define i32 @main() {
entry:
  store i64 5, i64* @i
  store double 6.700000e+00, double* @r
  %i = load i64, i64* @i
  %cmp = icmp slt i64 %i, 10
  store i1 %cmp, i1* @b
  store i8* getelementptr inbounds ([9 x i8], [9 x i8]* @.str.2, i32 0, i32 0), i8** @s
  %i1 = load i64, i64* @i
  %0 = call double @IntToReal(i64 %i1)
  store double %0, double* @r
  %r = load double, double* @r
  %1 = call i64 @RealToInt(double %r)
  store i64 %1, i64* @i
  %b = load i1, i1* @b
  %2 = call i8* @BoolToString(i1 %b)
  store i8* %2, i8** @s
  ret i32 0
}
