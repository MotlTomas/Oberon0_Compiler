; ModuleID = 'NestedProcTest'
source_filename = "NestedProcTest"
target triple = "x86_64-pc-linux-gnu"

@result = global i64 0
@.str = private unnamed_addr constant [20 x i8] c"Result of Outer(5):\00", align 1
@.fmt = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@.fmt.1 = private unnamed_addr constant [6 x i8] c"%lld\0A\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define i64 @Outer(i64) {
entry:
  %a.addr = alloca i64
  store i64 %0, i64* %a.addr
  %temp = alloca i64
  %a = load i64, i64* %a.addr
  %1 = call i64 @Inner(i64 %a)
  store i64 %1, i64* %temp
  %temp1 = load i64, i64* %temp
  %add = add i64 %temp1, 10
  ret i64 %add
}

define i64 @Inner(i64) {
entry:
  %b.addr = alloca i64
  store i64 %0, i64* %b.addr
  %b = load i64, i64* %b.addr
  %mul = mul i64 %b, 3
  ret i64 %mul
}

define i32 @main() {
entry:
  %0 = call i64 @Outer(i64 1)
  store i64 %0, i64* @result
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @.fmt, i32 0, i32 0), i8* getelementptr inbounds ([20 x i8], [20 x i8]* @.str, i32 0, i32 0))
  %result = load i64, i64* @result
  %2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([6 x i8], [6 x i8]* @.fmt.1, i32 0, i32 0), i64 %result)
  ret i32 0
}
