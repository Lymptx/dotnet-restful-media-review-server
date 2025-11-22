# Adds URL ACL for HttpListener
$port = 12000
$user = "$env:USERNAME"
netsh http add urlacl url="http://+:$port/" user=$user
