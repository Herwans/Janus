
# Janus

Small tool to mass rename elements in a folder.

## Usage

### Search pattern

Filter the folder's content following the input. Is plain text by default but can enable `Regex` mode by ticking the checkbox.

When `Case Sensitive` enable, only matching case will be displayed.

### Replace pattern

The input for the name replacement.

It's possible to add the current name of the file with `<current>` key. 

```
# Search
jewels

# Original name
I love those jewels

# Replace pattern
<current> with this dress

# Result
I love those jewels this dress
```

If `Regex` is enable, you can also use regex groups (indicated by parentheses around regex patterns).
You need to use `<g#>` while # indicate the position of the group in the regex, starting at 1

```
# Search
(those)(jewels)

# Original name
I love those jewels

# Replace pattern
I bought <g2> to match <g1> dresses

# Result
I bought jewels to match those dresses
```

If the `Remove search` is ticked, the search pattern will be remove from default replacement (where Replace pattern is empty) and from `<current>` key
## Authors

- [@Herwans](https://www.github.com/Herwans)


## License

[MIT](https://choosealicense.com/licenses/mit/)

