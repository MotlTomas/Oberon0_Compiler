; ModuleID = 'ArrayDemo'
source_filename = "ArrayDemo"
target triple = "x86_64-pc-linux-gnu"

@mat = global i8* null
@res = global i8* null
@.fmt = private unnamed_addr constant [5 x i8] c"%lld\00", align 1

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define void @FillMatrix(i8**) {
entry:
  %i = alloca i64
  %j = alloca i64
  store i64 0, i64* %i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i1 = load i64, i64* %i
  %for.cmp = icmp sle i64 %i1, 1
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  store i64 0, i64* %j
  br label %for.cond2

for.inc:                                          ; preds = %for.end5
  %i12 = load i64, i64* %i
  %for.inc13 = add i64 %i12, 1
  store i64 %for.inc13, i64* %i
  br label %for.cond

for.end:                                          ; preds = %for.cond

for.cond2:                                        ; preds = %for.inc4, %for.body
  %j6 = load i64, i64* %j
  %for.cmp7 = icmp sle i64 %j6, 1
  br i1 %for.cmp7, label %for.body3, label %for.end5

for.body3:                                        ; preds = %for.cond2
  %i8 = load i64, i64* %i
  %mul = mul i64 %i8, 10
  %j9 = load i64, i64* %j
  %add = add i64 %mul, %j9
  store i64 %add, i8** %0
  br label %for.inc4

for.inc4:                                         ; preds = %for.body3
  %j10 = load i64, i64* %j
  %for.inc11 = add i64 %j10, 1
  store i64 %for.inc11, i64* %j
  br label %for.cond2

for.end5:                                         ; preds = %for.cond2
  br label %for.inc
}

define i64 @SumMatrix(i8*) {
entry:
  %i = alloca i64
  %j = alloca i64
  %sum = alloca i64
  store i64 0, i64* %sum
  store i64 0, i64* %i
  br label %for.cond

for.cond:                                         ; preds = %for.inc, %entry
  %i1 = load i64, i64* %i
  %for.cmp = icmp sle i64 %i1, 1
  br i1 %for.cmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  store i64 0, i64* %j
  br label %for.cond2

for.inc:                                          ; preds = %for.end5
  %i11 = load i64, i64* %i
  %for.inc12 = add i64 %i11, 1
  store i64 %for.inc12, i64* %i
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %sum13 = load i64, i64* %sum
  ret i64 %sum13

for.cond2:                                        ; preds = %for.inc4, %for.body
  %j6 = load i64, i64* %j
  %for.cmp7 = icmp sle i64 %j6, 1
  br i1 %for.cmp7, label %for.body3, label %for.end5

for.body3:                                        ; preds = %for.cond2
  %sum8 = load i64, i64* %sum
  %a = load i8, i8* %0
  %add = add i64 %sum8, i8 %a
  store i64 %add, i64* %sum
  br label %for.inc4

for.inc4:                                         ; preds = %for.body3
  %j9 = load i64, i64* %j
  %for.inc10 = add i64 %j9, 1
  store i64 %for.inc10, i64* %j
  br label %for.cond2

for.end5:                                         ; preds = %for.cond2
  br label %for.inc
}

define i32 @main() {
entry:
  %mat = load i8*, i8** @mat
  call void @FillMatrix(i8* %mat)
  %mat1 = load i8*, i8** @mat
  store i8* %mat1, i8** @res
  %res = load i8*, i8** @res
  %0 = call i64 @SumMatrix(i8* %res)
  %1 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([5 x i8], [5 x i8]* @.fmt, i32 0, i32 0), i64 %0)
  ret i32 0
}
