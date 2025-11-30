; ModuleID = 'TypesDemo'
source_filename = "TypesDemo"

@i = global i32 0
@r = global double 0.000000e+00
@b = global i1 false
@s = global i8* null
@str = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str.1 = private unnamed_addr constant [6 x i8] c"false\00", align 1
@str.2 = private unnamed_addr constant [9 x i8] c"Result: \00", align 1
@str.3 = private unnamed_addr constant [9 x i8] c"Result: \00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

define double @IntToReal(i32) {
entry:
  %retval = alloca double
  %x = alloca i32
  store i32 %0, i32* %x
  %load = load i32, i32* %x
  store i32 %load, double* %retval
  br label %return

return:                                           ; preds = %entry

after_return:                                     ; No predecessors!
  unreachable
}

define i32 @RealToInt(double) {
entry:
  %retval = alloca i32
  %x = alloca double
  store double %0, double* %x
  %load = load double, double* %x
  store double %load, i32* %retval
  br label %return

return:                                           ; preds = %entry

after_return:                                     ; No predecessors!
  unreachable
}

define i8* @BoolToString(i1) {
entry:
  %retval = alloca i8*
  %val = alloca i1
  store i1 %0, i1* %val
  %load = load i1, i1* %val
  br i1 %load, label %if.then, label %if.else

return:                                           ; preds = %after_return, %if.then

if.then:                                          ; preds = %entry
  store i8* getelementptr inbounds ([5 x i8], [5 x i8]* @str, i32 0, i32 0), i8** %retval
  br label %return

if.merge:                                         ; No predecessors!

if.else:                                          ; preds = %entry

after_return:                                     ; No predecessors!
  unreachable
  store i8* getelementptr inbounds ([6 x i8], [6 x i8]* @str.1, i32 0, i32 0), i8** %retval
  br label %return

after_return1:                                    ; No predecessors!
  unreachable
  store i32 5, i32* @i
  store double 6.700000e+00, double* @r
  %load2 = load i32, i32* @i
  %cmplt = icmp slt i32 %load2, 10
  store i1 %cmplt, i1* @b
  store i8* getelementptr inbounds ([9 x i8], [9 x i8]* @str.2, i32 0, i32 0), i8** @s
  %load3 = load i32, i32* @i
  %call = call double (i32) @IntToReal(i32 %load3)
  store double (i32) %call, double* @r
  %load4 = load double, double* @r
  %call5 = call i32 (double) @RealToInt(double %load4)
  store i32 (double) %call5, i32* @i
  %load6 = load i1, i1* @b
  %call7 = call i8* (i1) @BoolToString(i1 %load6)
  store i8* (i1) %call7, i8** @s
}

define i32 @main() {
entry:
  store i32 5, i32* @i
  store double 6.700000e+00, double* @r
  %load = load i32, i32* @i
  %cmplt = icmp slt i32 %load, 10
  store i1 %cmplt, i1* @b
  store i8* getelementptr inbounds ([9 x i8], [9 x i8]* @str.3, i32 0, i32 0), i8** @s
  %load1 = load i32, i32* @i
  %call = call double (i32) @IntToReal(i32 %load1)
  store double (i32) %call, double* @r
  %load2 = load double, double* @r
  %call3 = call i32 (double) @RealToInt(double %load2)
  store i32 (double) %call3, i32* @i
  %load4 = load i1, i1* @b
  %call5 = call i8* (i1) @BoolToString(i1 %load4)
  store i8* (i1) %call5, i8** @s
  ret i32 0
}
