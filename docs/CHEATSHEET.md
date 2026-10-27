# Phrase cheat sheet

Indent 2 spaces. `#` comments. Keywords only at line start: module, type, phrase, endpoint, input, output, steps, let, respond with, fail with, fails with, effects, spec, given, when, then, and, native. Hole `{name: Type}`. Types: Text, Int, Decimal, Bool, Id, List<T>, Option<T>. `"t"` fits Text or Id; `1` fits Int or Decimal; `2.5` is Decimal; true/false are Bool; none fits only Option. Outcomes: returns, fails with, succeeds, responds. `when called` on a phrase; `when handled` on an endpoint. Verbs: GET POST PUT PATCH DELETE.

```phrase
module Shop

type Item
  name: Text
  qty: Int
  price: Decimal
  ok: Bool
  tag: Id

phrase "label of {item: Item}" -> Text
  effects Shelf
  fails with Missing
  spec "reads the name"
    given item = { name: "a", qty: 1, price: 2.5, ok: true, tag: "t-1" }
    when called
    then returns "a"
  native csharp
    return item.Name;

phrase "note of {note: Option<Text>}" -> Text
  spec "keeps text"
    given note = "n"
    when called
    then returns "n"
  native csharp
    return note ?? "";

phrase "sum of {left: Int} {right: Int}" -> Int
  spec "adds"
    given left = 1
    and right = 2
    when called
    then returns 3
  native csharp
    return left + right;

phrase "ensure {ok: Bool} holds"
  fails with Missing
  spec "rejects false"
    given ok = false
    when called
    then fails with Missing
  spec "holds"
    given ok = true
    when called
    then succeeds
  native csharp
    if (!ok) Fail("Missing");

phrase "count of {items: List<Item>}" -> Int
  native csharp
    return items.Count;

endpoint Place : POST /shop
  input
    id: Id
    items: List<Item>
  output
    n: Int
  steps
    let n = count of input.items
    respond with { n: n }
  spec "places one"
    given input { id: "i-1", items: [{ name: "a", qty: 1, price: 2.5, ok: true, tag: "t-1" }] }
    when handled
    then responds { n: 1 }
```
